'use strict';
UserController.$inject = ['$controller', 'fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$http', '$filter'];
function UserController($controller, fileReader, commonMessage, $scope, $rootScope, baseService, $routeParams, $http, $filter) {
    $rootScope.title = "User";
    $scope.compareTo = false;
    $scope.passwordShow = true;
    $scope.inactive = false;
    $scope.messaeShow = false;
    $scope.imageSrc = null;
    $scope.imageBtnDisable = false;
    $scope.Action = 'Save';
    $scope.companyGroupList = [];
    $scope.index = -1;
    $scope.users = [];
    $scope.path = 'Securities/user/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getEmployeeListUrl = 'employees/EmployeeInformation/GetWithoutUserEmployeeList';
    $scope.getUrl = $scope.path + 'get';
    $scope.getAuth = $scope.path + 'createauth';
    //$scope.getPin = $scope.path + 'createpin';
    $scope.getUserAccessFromEmp = $scope.path + 'getuseraccessfromemp';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $controller('organizationsBaseController', { $scope: $scope, $http: $http });
    baseService.init($scope.getListUrl, null, null, null, 'UserId', 'UserId');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.users = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    $rootScope.searchByUserList = [
        {
            'name': 'Username',
            'value': 'UserId'
        },
        {
            'name': 'User Type',
            'value': 'UserType'
        },
        {
            'name': 'Employee Id',
            'value': 'EmployeeId'
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
            'name': 'AuthToken',
            'value': 'AuthToken'
        }
    ];

    $scope.user = {
        Id: null
        , CompanyGroupId: null
        , UserId: null
        , EmployeeId: null
        , FullName: null
        , EmployeeCode: null
        , Password: null
        , ConfirmPassword: null
        , Image: null
        , LastPwdChangedDay: new Date()
        , DateOfBirth: null
        , Phone: null
        , Email: null
        , EmailVerified: false
        , EmailVerifiedDate: null
        , EmailVerificationCode: null
        , PasswordFailCount: 0
        , UserLocked: false
        , UserLockedDate: null
        , UserUnlockDate: null
        , AuthToken: null
        , AuthTokenFailCount: 0
        , AuthTokenLocked: false
        , AuthTokenLockedDate: null
        , AuthTokenUnlockDate: null
        , SysAdmin: false
        , PowerUser: false
        , GeneralUser: 'General User'
        , UserType: 'General User'
        , PwdChangeOnFirstLogin: false
        , PasswordNeverExpired: true
        , Remarks: null
        , Active: true
    };
    $scope.userNew = angular.copy($scope.user);

    $http({
        method: 'GET',
        url: 'Organizations/CompanyGroup/GetCbo'
    }).then(function successCallback(response) {
        $scope.companyGroupList = response.data;
    });

    $http({
        method: 'GET',
        url: $scope.getUserAccessFromEmp
    }).then(function successCallback(response) {
        if (response.data)
            $scope.searchEmployeeBtn = true;
        else
            $scope.searchEmployeeBtn = false;
    });

    $scope.compare = function (p1, p2) {
        $scope.result = angular.equals(p1, p2);
        if (!$scope.result)
            $scope.compareTo = true;
        else
            $scope.compareTo = false;
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.user = $scope.users[$scope.index];
        $scope.userNew = angular.copy($scope.user);
        if ($scope.userNew.SysAdmin)
            $scope.userNew.UserType = 'System Admin';
        else if ($scope.userNew.PowerUser)
            $scope.userNew.UserType = 'Power User';
        else
            $scope.userNew.UserType = 'General User';
        $scope.userNew.ConfirmPassword = $scope.userNew.Password;
        $scope.userNew.AddedDate = $filter('dateFiltering')($scope.userNew.AddedDate);
        $scope.userNew.UpdatedDate = $filter('dateFiltering')($scope.userNew.UpdatedDate);
        $scope.userNew.DateOfBirth = $filter('dateFiltering')($scope.userNew.DateOfBirth);
        $scope.userNew.EmailVerifiedDate = $filter('dateFilter')($scope.userNew.EmailVerifiedDate);
        $scope.userNew.UserLockedDate = $filter('dateFiltering')($scope.userNew.UserLockedDate);
        $scope.userNew.UserUnlockDate = $filter('dateFilter')($scope.userNew.UserUnlockDate);
        $scope.userNew.AuthTokenLockedDate = $filter('dateFilter')($scope.userNew.AuthTokenLockedDate);
        $scope.userNew.AuthTokenUnlockDate = $filter('dateFilter')($scope.userNew.AuthTokenUnlockDate);
        $scope.userNew.LastPwdChangedDay = $filter('dateFilter')($scope.userNew.LastPwdChangedDay);
        $scope.imageSrc = virtualPath.EmployeeImage + $scope.userNew.Image;
        if (!baseService.isUndefinedOrNull($scope.userNew.EmployeeId))
            $scope.imageBtnDisable = true;
        $scope.inactive = true;
        $scope.passwordShow = false;
        $rootScope.lengthCheck = false;
        $scope.getUserSalesGroupList();
        $scope.getUserPurchaseGroupList();
        getUserSectionList();
        getUserPayrollGroupList();
        getUserProcessList();
        getUserGateList();
        getUserSFGInventoryList();
        getuserReportGroupList();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        if ($scope.userNew.SysAdmin === true && $scope.userNew.PowerUser === true) {
            return ShowResult('You can not be checked system Admin and power user both.', "failure");
        }
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.userNewForm.$valid) {
            if ($scope.userNew.UserType === 'System Admin') {
                $scope.userNew.SysAdmin = true;
                $scope.userNew.PowerUser = false;
            }
            else if ($scope.userNew.UserType === 'Power User') {
                $scope.userNew.SysAdmin = false;
                $scope.userNew.PowerUser = true;
            }
            else {
                $scope.userNew.SysAdmin = false;
                $scope.userNew.PowerUser = false;
            }
            var formData = new FormData();
            angular.copy($scope.userNew, $scope.user);
            //$scope.user.DateOfBirth = $filter('dateFilter')($scope.user.DateOfBirth);
            $scope.user.EmailVerifiedDate = $filter('dateFilter')($scope.user.LastPwdChangedDay);
            $scope.user.UserLockedDate = $filter('dateFilter')($scope.user.LastPwdChangedDay);
            $scope.user.UserUnlockDate = $filter('dateFilter')($scope.user.LastPwdChangedDay);
            $scope.user.AuthTokenLockedDate = $filter('dateFilter')($scope.user.LastPwdChangedDay);
            $scope.user.AuthTokenUnlockDate = $filter('dateFilter')($scope.user.LastPwdChangedDay);
            $scope.user.LastPwdChangedDay = $filter('dateFilter')($scope.user.LastPwdChangedDay);
            if (!angular.equals($scope.user.Password, $scope.user.ConfirmPassword))
                return ShowResult('Confirm password does not match.', 'failure');
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        formData.append("user", angular.toJson(data.user));
                        formData.append("userSalesGroup", JSON.stringify(data.userSalesGroup));
                        formData.append("userPurchaseGroup", JSON.stringify(data.userPurchaseGroup));
                        formData.append("sectionList", JSON.stringify(data.sectionList));
                        formData.append("payrollGroupList", JSON.stringify(data.payrollGroupList));
                        formData.append("userProcessList", JSON.stringify(data.userProcessList));
                        formData.append("userGateList", JSON.stringify(data.userGateList));
                        formData.append("userSFGInventoryList", JSON.stringify(data.userSFGInventoryList));
                        formData.append("userReportGroupList", JSON.stringify(data.userReportGroupList));
                        formData.append('file', data.file);
                        return formData;
                    },
                    data: {
                        'user': $scope.user
                        , 'userSalesGroup': $scope.userSalesGroupList
                        , 'userPurchaseGroup': $scope.userPurchaseGroupList
                        , 'sectionList': $scope.sectionList
                        , 'payrollGroupList': $scope.payrollGroupList
                        , 'userProcessList': $scope.userProcessList
                        , 'userGateList': $scope.userGateList
                        , 'userSFGInventoryList': $scope.userSFGInventoryList
                        , 'userReportGroupList': $scope.userReportGroupList
                        , 'file': $scope.filedata
                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');

                        if ($scope.userNew.SysAdmin)
                            response.data.User['UserType'] = 'System Admin';
                        else if ($scope.userNew.PowerUser)
                            response.data.User['UserType'] = 'Power User';
                        else
                            response.data.User['UserType'] = 'General User';

                        $scope.users.push(response.data.User);
                        baseService.paginationAdd();
                        ClearFields(response.data.AuthToken);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        formData.append("user", angular.toJson(data.user));
                        formData.append("userSalesGroup", JSON.stringify(data.userSalesGroup));
                        formData.append("userPurchaseGroup", JSON.stringify(data.userPurchaseGroup));
                        formData.append("sectionList", JSON.stringify(data.sectionList));
                        formData.append("payrollGroupList", JSON.stringify(data.payrollGroupList));
                        formData.append("userProcessList", JSON.stringify(data.userProcessList));
                        formData.append("userGateList", JSON.stringify(data.userGateList));
                        formData.append("userSFGInventoryList", JSON.stringify(data.userSFGInventoryList));
                        formData.append("userReportGroupList", JSON.stringify(data.userReportGroupList));
                        formData.append('file', data.file);
                        return formData;
                    },
                    data: {
                        'user': $scope.user
                        , 'userSalesGroup': $scope.userSalesGroupList
                        , 'userPurchaseGroup': $scope.userPurchaseGroupList
                        , 'sectionList': $scope.sectionList
                        , 'payrollGroupList': $scope.payrollGroupList
                        , 'userProcessList': $scope.userProcessList
                        , 'userGateList': $scope.userGateList
                        , 'userSFGInventoryList': $scope.userSFGInventoryList
                        , 'userReportGroupList': $scope.userReportGroupList
                        , 'file': $scope.filedata
                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            //if ($scope.user.SysAdmin)
                            //    $scope.user.UserType = 'System Admin';
                            //else if ($scope.user.PowerUser)
                            //    $scope.user.UserType = 'Power User';
                            //else
                            //    $scope.user.UserType = 'General User';

                            $scope.users[$scope.index] = $scope.user;
                        }
                        ClearFields(response.data.AuthToken);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.Message, 'failure');
                });
                return true;
            }
        }
        else
            $scope.setTab(1);
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.userNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.userNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.users.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.AuthToken);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };

    $scope.AuthToken = function () {
        $http.get($scope.getAuth)
            .then(function (response) {
                $scope.userNew.AuthToken = response.data;
            });
    };
    $scope.AuthToken();

    $scope.Clear = function () {
        ClearFields($scope.AuthToken());
        return true;
    };

    $scope.clearImage = function () {
        $scope.imageSrc = '';
        $scope.userNew.Image = '';
        document.getElementById("uploadImage").value = '';
        document.getElementById("uploadImageSrc").setAttribute('src', null);
    };

    function ClearFields(auth) {
        $scope.clearImage();
        $scope.imageBtnDisable = false;
        $scope.Action = "Save";
        $scope.index = -1;
        $scope.user = {};
        $scope.userNew = angular.copy($scope.user);
        $scope.userNew.Id = null;
        $scope.userNew.SysAdmin = false;
        $scope.userNew.PowerUser = false;
        $scope.userNew.GeneralUser = 'General User';
        $scope.userNew.UserType = 'General User';
        $scope.userNew.AuthToken = auth;
        $scope.userNew.PasswordChangeOnFirstLogin = true;
        $scope.userNew.PasswordNeverExpired = true;
        $scope.userNew.Active = true;
        $scope.inactive = false;
        $scope.passwordShow = true;
        $scope.fullName = false;
        $scope.dOB = false;
        $rootScope.lengthCheck = false;
        $scope.userSalesGroupTblShow = false;
        $scope.userSalesGroupList = [];
        $scope.userPurchaseGroupList = [];
        $scope.sectionList = [];
        $scope.payrollGroupList = [];
        $scope.userProcessList = [];
        $scope.userGateList = [];
        $scope.userSFGInventoryList = [];
        $scope.userPurchaseGroupTblShow = false;
        $scope.setTab(1);
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.employeeList = [];
    $scope.employeeParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeCodeNumeric',
        searchBy: "EmployeeCode",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.ShowEmployeeListPopUp = function () {
        $scope.getEmployeeData = function (pageno) {
            baseService.paginationBase($scope.getEmployeeListUrl, pageno, $scope.employeeParameters)
                .then(function (result) {
                    $scope.employeeList = result.Rows;
                    $scope.employeeParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#employeePopUp')).modal('show');
        $scope.getEmployeeData();
    };

    $scope.selectEmployeedblClick = function (data) {
        $scope.userNew.EmployeeId = data.SystemID;
        $scope.userNew.FullName = data.EmployeeName;
        $scope.userNew.EmployeeCode = data.EmployeeCode;
        $scope.userNew.Active = true;
        $scope.userNew.Email = data.EmailId;
        $scope.userNew.Phone = data.CellPhnNo;
        $scope.userNew.DateOfBirth = $filter('dateFiltering')(data.DateOfBirth);
        $scope.imageSrc = virtualPath.EmployeePic + data.Image;
        $scope.fullName = true;
        $scope.dOB = true;
        $scope.employeeData = '';
        setUserImage(data);
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    };

    function setUserImage(data) {
        if (!baseService.isUndefinedOrNull(data.SystemId)) {
            $scope.imageSrc = virtualPath.EmployeePic + data.Image;
            $scope.imageBtnDisable = true;
            $scope.userNew.Image = data.Image;
            $scope.filedata = null;
        }
        else
            $scope.userNew.Image = null;
    }

    $scope.filedata = null;
    $("#uploadImage").change(function () {
        $scope.filedata = this.files[0];
    });

    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };

    $scope.employeeData = '';
    $scope.selectEmplyee = function (data) {
        $scope.employeeData = data;
    };

    $scope.SelectEmployeeByButton = function () {
        if ($scope.employeeId === '') {
            return ShowResult('Please at first select row', 'failure', 'employeePopUp');
        }
        $scope.selectEmployeedblClick($scope.employeeData);
        $scope.employeeData = '';
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    };

    $scope.CloseEmployeePopUp = function () {
        $scope.employeeId = '';
        $scope.FullName = '';
        $scope.DateOfBirth = '';
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    };

    $rootScope.searchEmployeeList = [
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'First Name',
            'value': 'FirstName'
        },
        {
            'name': 'Middle Name',
            'value': 'MiddleName'
        },
        {
            'name': 'Last Name',
            'value': 'LastName'
        },
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
        },

    ];

    // #endregion

    // #region Sales Group
    $scope.userSalesGroupList = [];
    $scope.userSalesGroupTblShow = false;
    $scope.salesGroups = [];
    $scope.getUserSalesGroupList = function () {
        $scope.userSalesGroupTblShow = true;
        $http({
            method: 'GET',
            url: 'Securities/user/getusersalesgrouplist?userid=' + $scope.userNew.Id
        }).then(function successCallback(response) {
            $scope.userSalesGroupList = response.data;
        });
    };

    $scope.salesOrganizationList = [];
    $http({
        method: 'GET',
        url: 'Organizations/salesorganisation/getcbo'
    }).then(function successCallback(response) {
        $scope.salesOrganizationList = response.data;
    });
    $scope.salesGroupParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.SalesGroupPopUp = function () {
        $scope.getSalesGroupData = function (pageno) {
            $scope.getSalesGroupUrl = 'Organizations/salesgroup/searchsalesgrouplist?salesOrganizationId=' + $scope.SalesOrganizationId
                + '&salesGroupIds=' + isSalesGroupIdExistInUser($scope.userSalesGroupList);
            baseService.paginationBase($scope.getSalesGroupUrl, pageno, $scope.salesGroupParameters)
                .then(function (result) {
                    $scope.salesGroups = result.Rows;
                    $scope.salesGroupParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#salesGroupPopUp')).modal('show');
        $scope.getSalesGroupData();
    };

    $scope.CloseSalesGroupPopUp = function () {
        angular.element(document.querySelector('#salesGroupPopUp')).modal('hide');
    };

    $rootScope.searchSalesGroupByList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        }
    ];
    function isSalesGroupIdExistInUser(list) {
        $scope.salesGroupIds = [];
        if (list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                if (list[i]['Archive'] === false) {
                    $scope.salesGroupIds.push(list[i]['SalesGroupId']);
                }
            }
        }
        return JSON.stringify($scope.salesGroupIds);
    }
    $scope.addSalesGroup = function () {
        if (!isRowSelected($scope.salesGroups)) {
            ShowResult('Please select at least one row', 'failure', 'salesGroupPopUp');
            return;
        }
        $scope.salesOrganizationName = document.getElementById("salesOrganization").options[document.getElementById('salesOrganization').selectedIndex].text;
        angular.forEach($scope.salesGroups, function (a) {
            if (a.Flag) {
                $scope.userSalesGroupList.push({
                    Id: null,
                    SalesGroupId: a.Id,
                    UserId: $scope.userNew.Id,
                    Code: a.Code,
                    ShortName: a.ShortName,
                    StandardName: a.StandardName,
                    SalesOrganizationName: $scope.salesOrganizationName,
                    SalesGroupName: a.UserName,
                    Archive: false
                });
            }
        });
        if (!$scope.userSalesGroupTblShow)
            $scope.userSalesGroupTblShow = true;
        $scope.CloseSalesGroupPopUp();
    };
    function isRowSelected(ilst) {
        try {
            var flag = false;
            for (var i = 0; i < ilst.length; i++) {
                if (ilst[i].Flag) {
                    return flag = true;
                }
            }
        } catch (e) {
            throw e;
        }
    }
    $scope.valuePassInDelModal = function (salesGroupId, name, index) {
        $scope.message_confirmation = '';
        $scope.index = index;
        $scope.salesGroupId = salesGroupId;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + name + ' ]';
        angular.element(document.querySelector('#confirmSalesPopUp')).modal('show');
    };
    $scope.removeUserSalesGroupRow = function () {
        for (var i = 0; i < $scope.userSalesGroupList.length; i++) {
            if ($scope.userSalesGroupList[i].Id === null && $scope.userSalesGroupList[i].SalesGroupId === $scope.salesGroupId) {
                $scope.userSalesGroupList.splice(i, 1);
            }
            else if ($scope.userSalesGroupList[i].Id !== null && $scope.userSalesGroupList[i].SalesGroupId === $scope.salesGroupId)
                $scope.userSalesGroupList[i].Archive = true;
        }
        if ($scope.userSalesGroupList.length > 0) {
            $scope.userSalesGroupTblShow = true;
        }
        else {
            $scope.userSalesGroupTblShow = false;
        }
        $scope.index = -1;
    };
    // #endregion


    // #region Purchase Group
    $scope.userPurchaseGroupList = [];
    $scope.userPurchaseGroupTblShow = false;
    $scope.purchaseGroups = [];
    $scope.getUserPurchaseGroupList = function () {
        $scope.userPurchaseGroupTblShow = true;
        $http({
            method: 'GET',
            url: 'Securities/user/getuserpurchasegrouplist?userid=' + $scope.userNew.Id
        }).then(function successCallback(response) {
            $scope.userPurchaseGroupList = response.data;
        });
    };

    $scope.purchaseOrganizationList = [];
    $http({
        method: 'GET',
        url: 'Organizations/purchaseorganisation/getcbo'
    }).then(function successCallback(response) {
        $scope.purchaseOrganizationList = response.data;
    });

    $scope.purchaseGroupParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.PurchaseGroupPopUp = function () {
        $scope.getPurchaseGroupData = function (pageno) {
            $scope.getPurchaseGroupUrl = 'Organizations/Purchasegroup/searchPurchasegrouplist?purchaseOrganizationId=' + $scope.purchaseOrganizationId
                + '&purchaseGroupIds=' + isPurchaseGroupIdExistInUser($scope.userPurchaseGroupList);
            baseService.paginationBase($scope.getPurchaseGroupUrl, pageno, $scope.purchaseGroupParameters)
                .then(function (result) {
                    $scope.purchaseGroups = result.Rows;
                    $scope.purchaseGroupParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#purchaseGroupPopUp')).modal('show');
        $scope.getPurchaseGroupData();
    };

    $scope.ClosePurchaseGroupPopUp = function () {
        angular.element(document.querySelector('#purchaseGroupPopUp')).modal('hide');
    };

    $rootScope.searchPurchaseGroupByList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        }
    ];
    function isPurchaseGroupIdExistInUser(list) {
        $scope.purchaseGroupIds = [];
        if (list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                if (list[i]['Archive'] === false) {
                    $scope.purchaseGroupIds.push(list[i]['PurchaseGroupId']);
                }
            }
        }
        return JSON.stringify($scope.purchaseGroupIds);
    }
    $scope.addPurchaseGroup = function () {
        if (!isRowSelected($scope.purchaseGroups)) {
            ShowResult('Please select at least one row', 'failure', 'purchaseGroupPopUp');
            return;
        }
        $scope.purchacseOrganizationName = document.getElementById("purchacseOrganization").options[document.getElementById('purchacseOrganization').selectedIndex].text;
        angular.forEach($scope.purchaseGroups, function (a) {
            if (a.Flag) {
                $scope.userPurchaseGroupList.push({
                    Id: null,
                    PurchaseGroupId: a.Id,
                    UserId: $scope.userNew.Id,
                    Code: a.Code,
                    ShortName: a.ShortName,
                    StandardName: a.StandardName,
                    PurchaseOrganizationName: $scope.purchacseOrganizationName,
                    PurchaseGroupName: a.UserName,
                    Archive: false
                });
            }
        });
        if (!$scope.userPurchaseGroupTblShow)
            $scope.userPurchaseGroupTblShow = true;
        $scope.ClosePurchaseGroupPopUp();
    };

    $scope.purchaseDelModal = function (purchaseGroupId, name, index) {
        $scope.message_confirmation = '';
        $scope.index = index;
        $scope.purchaseGroupId = purchaseGroupId;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + name + ' ]';
        angular.element(document.querySelector('#confirmPurchasePopUp')).modal('show');
    };
    $scope.removeUserPurchaseGroupRow = function () {
        for (var i = 0; i < $scope.userPurchaseGroupList.length; i++) {
            if ($scope.userPurchaseGroupList[i].Id === null && $scope.userPurchaseGroupList[i].PurchaseGroupId === $scope.purchaseGroupId) {
                $scope.userPurchaseGroupList.splice(i, 1);
            }
            else if ($scope.userPurchaseGroupList[i].Id !== null && $scope.userPurchaseGroupList[i].PurchaseGroupId === $scope.purchaseGroupId)
                $scope.userPurchaseGroupList[i].Archive = true;
        }
        if ($scope.userPurchaseGroupList.length > 0) {
            $scope.userPurchaseGroupTblShow = true;
        }
        else {
            $scope.userPurchaseGroupTblShow = false;
        }
        $scope.index = -1;
    };

    // #endregion

    // #region Section

    $scope.sectionList = [];
    $scope.addSection = function () {
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            angular.forEach($rootScope.tempList, function (a) {
                if (!baseService.valueCheckInList($scope.sectionList, 'SectionId', a.Id)) {
                    $scope.sectionList.push({
                        Id: null
                        , SectionId: a.Id
                        , UserId: $scope.userNew.Id
                        , Sequence: a.Sequence
                        , Code: a.Code
                        , ShortName: a.ShortName
                        , UserName: a.UserName
                        , Active: a.Active
                    });
                }
            });
        }
        else
            $scope.sectionList = [];
        angular.forEach($scope.sectionList, function (a) {
            if (!baseService.valueCheckInList($rootScope.tempList, 'Id', a.SectionId))
                $scope.sectionList.splice(a, 1);
        });
        $scope.closeSectionPopUp();
    };
    $scope.removeRowModal = function (name, index, listName, tempId, listId) {
        try {
            $scope.popUpIndex = index;
            $scope.listName = listName;
            $scope.tempId = tempId;
            $scope.listId = listId;
            $scope.message_confirmation = "Are you sure want to permanent delete [" + name + "] ";
            angular.element(document.querySelector('#confirmRemovePopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeRow = function () {
        for (var t = 0; t < baseService.arrayLength($rootScope.tempList); t++) {
            if ($rootScope.tempList[t][$scope.tempId] === $scope[$scope.listName][$scope.popUpIndex][$scope.listId])
                $rootScope.tempList.splice(t, 1);
        }
        $scope[$scope.listName].splice($scope.popUpIndex, 1);
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#confirmRemovePopUp')).modal('hide');
    };

    function getUserSectionList() {
        $http({
            method: 'GET',
            url: 'Securities/user/GetUserSectionList?userid=' + $scope.userNew.Id
        }).then(function successCallback(response) {
            $scope.sectionList = response.data;
        });
    }

    // #endregion Section

    // #region Payroll

    $scope.payrollGroupList = [];

    $scope.payrollPopUpDataList = function () {
        $scope.payrollDataList = [];
        $scope.payrollSearchList = [];
        $rootScope.tempList = [];
        CloseShowResult();
        CloseModalShowResult();
        $scope.payrollPopUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: "UserName"
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };

        $scope.payrollUpUrl = 'Setups/EmployeeAttendanceGroup/GetListWithUser?userId=' + $scope.userNew.Id;

        baseService.setCurrentPage('payrollDataList');
        $scope.getPayrollDataList = function (pageno) {
            baseService.paginationBase($scope.payrollUpUrl, pageno, $scope.payrollPopUpParameters)
                .then(function (result) {
                    $scope.payrollDataList = result.Rows;
                    $scope.payrollPopUpParameters.total_count = result.Total;

                    if (baseService.arrayLength($scope.payrollGroupList) > 0) {
                        for (var i = 0; i < $scope.payrollGroupList.length; i++) {
                            for (var j = 0; j < $scope.payrollDataList.length; j++) {
                                if ($scope.payrollGroupList[i].PayrollGroupId === $scope.payrollDataList[j].Id) {
                                    $scope.payrollDataList[j].Flag = true;
                                }
                            }
                        }
                    }
                    if (baseService.arrayLength($scope.payrollSearchList) === 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.payrollSearchList);
                    angular.element(document.querySelector('#payrollPopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'payrollPopUp');
                }).finally(function () {
                });

        };
        $scope.getPayrollDataList();
    };


    $scope.addPayroll = function () {
        if (baseService.arrayLength($scope.payrollDataList) > 0) {
            angular.forEach($scope.payrollDataList, function (a) {
                if (checkpayrollGroupExist($scope.payrollGroupList, a.Id) === false) {
                    if (a.Flag) {
                        $scope.payrollGroupList.push({
                            Id: null
                            , UserId: $scope.userNew.Id
                            , PayrollGroupId: a.Id
                            , EmployeeId: $scope.userNew.EmployeeId
                            , Code: a.Code
                            , Sequence: a.Sequence
                            , ShortName: a.ShortName
                            , StandardName: a.StandardName
                            , PayrollGroupName: a.UserName
                        });
                    }
                }

            });
        }
        else
            //$scope.payrollGroupList = [];
            angular.forEach($scope.payrollGroupList, function (a) {
                if (!baseService.valueCheckInList($scope.payrollDataList, 'Id', a.PayrollGroupId))
                    $scope.payrollGroupList.splice(a, 1);
            });
        $scope.closePayrollPopUp();
    };

    function checkpayrollGroupExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].PayrollGroupId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.closePayrollPopUp = function () {
        $scope.payrollUpUrl = null;
        $scope.payrollDataList = [];
        $scope.payrollSearchList = [];
        angular.element(document.querySelector('#payrollPopUp')).modal('hide');
    };

    function getUserPayrollGroupList() {
        $http({
            method: 'GET',
            url: 'Securities/user/UserPayrollGroupList?userid=' + $scope.userNew.Id
        }).then(function successCallback(response) {
            $scope.payrollGroupList = response.data;
        });
    }

    // #endregion Payroll

    // #region Process

    $scope.userProcessList = [];

    $scope.processPopUpDataList = function () {
        $scope.processDataList = [];
        $scope.processSearchList = [];
        $rootScope.tempList = [];
        CloseShowResult();
        CloseModalShowResult();
        $scope.processPopUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'UserName'
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.processUrl = 'Processes/Process/GetList?processId=[]';
        baseService.setCurrentPage('processDataList');
        $scope.getProcessDataList = function (pageno) {
            baseService.paginationBase($scope.processUrl, pageno, $scope.processPopUpParameters)
                .then(function (result) {
                    $scope.processDataList = result.Rows;
                    $scope.processPopUpParameters.total_count = result.Total;

                    if (baseService.arrayLength($scope.userProcessList) > 0) {
                        for (var i = 0; i < $scope.userProcessList.length; i++) {
                            for (var j = 0; j < $scope.processDataList.length; j++) {
                                if ($scope.userProcessList[i].ProcessId === $scope.processDataList[j].Id) {
                                    $scope.processDataList[j].Flag = true;

                                }
                            }
                        }
                    }

                    if (baseService.arrayLength($scope.processSearchList) === 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.processSearchList);
                    angular.element(document.querySelector('#processPopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'processPopUp');
                }).finally(function () {
                });
        };
        $scope.getProcessDataList();
    };

    $scope.addProcess = function () {
        if (baseService.arrayLength($scope.processDataList) > 0) {
            angular.forEach($scope.processDataList, function (a) {
                if (checkProcessExist($scope.userProcessList, a.Id) === false) {
                    if (a.Flag) {
                        $scope.userProcessList.push({
                            Id: null
                            , ProcessId: a.Id
                            , UserId: $scope.userNew.Id
                            , Code: a.Code
                            , Sequence: a.Sequence
                            , ShortName: a.ShortName
                            , StandardName: a.StandardName
                            , ProcessName: a.UserName
                        });
                    }
                }

            });
        }
        //else
        //    $scope.userProcessList = [];
        //angular.forEach($scope.userProcessList, function (a) {
        //    if (!baseService.valueCheckInList($scope.processDataList, 'Id', a.ProcessId))
        //        $scope.userProcessList.splice(a, 1);
        //});
        $scope.closeProcessPopUp();
    };

    function checkProcessExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ProcessId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.closeProcessPopUp = function () {
        $scope.processUpUrl = null;
        $scope.processDataList = [];
        $scope.processSearchList = [];
        angular.element(document.querySelector('#processPopUp')).modal('hide');
    };

    function getUserProcessList() {
        $http({
            method: 'GET',
            url: 'Securities/user/getUserProcessList?userid=' + $scope.userNew.Id
        }).then(function successCallback(response) {
            $scope.userProcessList = response.data;
        });
    }

    // #endregion Process

    // #region Gate

    $scope.userGateList = [];

    $scope.gatePopUpDataList = function () {
        $scope.gateDataList = [];
        $scope.gateSearchList = [];
        $rootScope.tempList = [];
        CloseShowResult();
        CloseModalShowResult();
        $scope.gatePopUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'UserName'
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.gateUrl = 'Products/PlantWiseGate/GetGateData';
        baseService.setCurrentPage('gateDataList');
        $scope.getGateDataList = function (pageno) {
            baseService.paginationBase($scope.gateUrl, pageno, $scope.gatePopUpParameters)
                .then(function (result) {
                    $scope.gateDataList = result.Rows;
                    $scope.gatePopUpParameters.total_count = result.Total;

                    if (baseService.arrayLength($scope.userGateList) > 0) {
                        for (var i = 0; i < $scope.userGateList.length; i++) {
                            for (var j = 0; j < $scope.gateDataList.length; j++) {
                                if ($scope.userGateList[i].PlantGateId === $scope.gateDataList[j].Id) {
                                    $scope.gateDataList[j].Flag = true;
                                }
                            }
                        }
                    }

                    if (baseService.arrayLength($scope.gateSearchList) === 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.gateSearchList);
                    angular.element(document.querySelector('#gatePopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'gatePopUp');
                }).finally(function () {
                });
        };
        $scope.getGateDataList();
    };

    $scope.addGate = function () {
        if (baseService.arrayLength($scope.gateDataList) > 0) {
            angular.forEach($scope.gateDataList, function (a) {
                if (checkGateExist($scope.userGateList, a.Id) === false) {
                    if (a.Flag) {
                        $scope.userGateList.push({
                            Id: null,
                            PlantGateId: a.Id
                            , UserId: $scope.userNew.Id
                            , Code: a.Code
                            , Sequence: a.Sequence
                            , ShortName: a.ShortName
                            , GateName: a.UserName
                            , StandardName: a.StandardName
                        });
                    }
                }

            });
        }
        else
            // $scope.userGateList = [];
            angular.forEach($scope.userGateList, function (a) {
                if (!baseService.valueCheckInList($scope.gateDataList, 'Id', a.PlantGateId))
                    $scope.userGateList.splice(a, 1);
            });
        $scope.closeGatePopUp();
    };

    $scope.closeGatePopUp = function () {
        $scope.gateUpUrl = null;
        $scope.gateDataList = [];
        $scope.gateSearchList = [];
        angular.element(document.querySelector('#gatePopUp')).modal('hide');
    };

    function getUserGateList() {
        $http({
            method: 'GET',
            url: 'Products/PlantWiseGate/GetUserGateList?userid=' + $scope.userNew.Id
        }).then(function successCallback(response) {
            $scope.userGateList = response.data;
        });
    }

    function checkGateExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].PlantGateId === Id) {
                return true;
            }
        }
        return false;
    }

    // #endregion Gate

    // #region UserSFGInventory

    $scope.userSFGInventoryList = [];
    $scope.SFGInventoryPopUpDataList = function () {
        $scope.SFGInventoryDataList = [];
        $scope.SFGInventorySearchList = [];
        $rootScope.tempList = [];
        CloseShowResult();
        CloseModalShowResult();
        $scope.SFGInventoryPopUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'UserName'
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.SFGInventoryUrl = 'Products/SFGInventory/GetList';

        baseService.setCurrentPage('SFGInventoryDataList');
        $scope.getSFGInventoryDataList = function (pageno) {
            baseService.paginationBase($scope.SFGInventoryUrl, pageno, $scope.SFGInventoryPopUpParameters)
                .then(function (result) {
                    $scope.SFGInventoryDataList = result.Rows;
                    $scope.SFGInventoryPopUpParameters.total_count = result.Total;

                    if (baseService.arrayLength($scope.userSFGInventoryList) > 0) {
                        for (var i = 0; i < $scope.userSFGInventoryList.length; i++) {
                            for (var j = 0; j < $scope.SFGInventoryDataList.length; j++) {
                                if ($scope.userSFGInventoryList[i].SFGInventoryId === $scope.SFGInventoryDataList[j].Id) {
                                    $scope.SFGInventoryDataList[j].Flag = true;
                                }
                            }
                        }
                    }


                    if (baseService.arrayLength($scope.SFGInventorySearchList) === 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.SFGInventorySearchList);
                    angular.element(document.querySelector('#SFGInventoryPopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'SFGInventoryPopUp');
                }).finally(function () {
                });
        };
        $scope.getSFGInventoryDataList();
    };

    $scope.addSFGInventory = function () {
        if (baseService.arrayLength($scope.SFGInventoryDataList) > 0) {
            angular.forEach($scope.SFGInventoryDataList, function (a) {
                // if (!baseService.valueCheckInList($scope.userSFGInventoryList, 'SFGInventoryId', a.Id)) {
                if (checkSFGInventoryExist($scope.userSFGInventoryList, a.Id) === false) {
                    if (a.Flag) {
                        $scope.userSFGInventoryList.push({
                            Id: null
                            , SFGInventoryId: a.Id
                            , UserId: $scope.userNew.Id
                            , Code: a.Code
                            , Sequence: a.Sequence
                            , ShortName: a.ShortName
                            , UserName: a.UserName
                            , StandardName: a.StandardName
                        });
                    }
                }
            });
        }
        else
            $scope.userSFGInventoryList = [];
        angular.forEach($scope.userSFGInventoryList, function (a) {
            if (!baseService.valueCheckInList($scope.SFGInventoryDataList, 'Id', a.SFGInventoryId))
                $scope.userSFGInventoryList.splice(a, 1);
        });
        $scope.closeSFGInventoryPopUp();
    };

    $scope.closeSFGInventoryPopUp = function () {
        $scope.SFGInventoryUpUrl = null;
        $scope.SFGInventoryDataList = [];
        $scope.SFGInventorySearchList = [];
        angular.element(document.querySelector('#SFGInventoryPopUp')).modal('hide');
    };

    function getUserSFGInventoryList() {
        $http({
            method: 'GET',
            url: 'Products/SFGMovement/GetUserSFGMovementList?userid=' + $scope.userNew.Id
        }).then(function successCallback(response) {
            $scope.userSFGInventoryList = response.data;
        });
    }

    function checkSFGInventoryExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SFGInventoryId === Id) {
                return true;
            }
        }
        return false;
    }

    // #endregion Gate

    // #region ReportGroup

    $scope.userReportGroupList = [];

    $scope.ReportGroupPopUpDataList = function () {
        $scope.ReportGroupDataList = [];
        $scope.ReportGroupSearchList = [];
        $rootScope.tempList = [];
        CloseShowResult();
        CloseModalShowResult();
        $scope.ReportGroupPopUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'UserName'
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.ReportGroupUrl = 'Setups/ReportingGroup/GetReportingGroupList';
        baseService.setCurrentPage('ReportGroupDataList');
        $scope.getReportGroupDataList = function (pageno) {
            baseService.paginationBase($scope.ReportGroupUrl, pageno, $scope.ReportGroupPopUpParameters)
                .then(function (result) {
                    $scope.ReportGroupDataList = result.Rows;
                    $scope.ReportGroupPopUpParameters.total_count = result.Total;

                    if (baseService.arrayLength($scope.userReportGroupList) > 0) {
                        for (var i = 0; i < $scope.userReportGroupList.length; i++) {
                            for (var j = 0; j < $scope.ReportGroupDataList.length; j++) {
                                if ($scope.userReportGroupList[i].ReportingGroupId === $scope.ReportGroupDataList[j].Id) {
                                    $scope.ReportGroupDataList[j].Flag = true;
                                }
                            }
                        }
                    }

                    if (baseService.arrayLength($scope.ReportGroupSearchList) === 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.ReportGroupSearchList);
                    angular.element(document.querySelector('#ReportGroupPopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'ReportGroupPopUp');
                }).finally(function () {
                });
        };
        $scope.getReportGroupDataList();
    };

    $scope.addReportingGroup = function () {
        if (baseService.arrayLength($scope.ReportGroupDataList) > 0) {
            angular.forEach($scope.ReportGroupDataList, function (a) {
                if (checkReportingGroupExist($scope.userReportGroupList, a.Id) === false) {
                    if (a.Flag) {
                        $scope.userReportGroupList.push({
                            Id: null
                            , ReportingGroupId: a.Id
                            , UserId: $scope.userNew.Id
                            , Code: a.Code
                            , Sequence: a.Sequence
                            , ShortName: a.ShortName
                            , StandardName: a.StandardName
                            , UserName: a.UserName
                        });
                    }
                }

            });
        }
        else
            $scope.userReportGroupList = [];
        angular.forEach($scope.userReportGroupList, function (a) {
            if (!baseService.valueCheckInList($scope.ReportGroupDataList, 'Id', a.ReportingGroupId))
                $scope.userReportGroupList.splice(a, 1);
        });
        $scope.closeReportGroupPopUp();
    };

    function checkReportingGroupExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ReportingGroupId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.closeReportGroupPopUp = function () {
        $scope.ReportGroupUpUrl = null;
        $scope.ReportGroupDataList = [];
        $scope.ReportGroupSearchList = [];
        angular.element(document.querySelector('#ReportGroupPopUp')).modal('hide');
    };

    function getuserReportGroupList() {
        $http({
            method: 'GET',
            url: 'Setups/ReportingGroup/getuserReportGroupList?userid=' + $scope.userNew.Id
        }).then(function successCallback(response) {
            $scope.userReportGroupList = response.data;
        });
    }

    // #endregion ReportGroup
}