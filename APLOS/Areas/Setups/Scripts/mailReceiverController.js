'use strict';
mailReceiverController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function mailReceiverController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "Mail Receiver";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.brands = [];
    $scope.toList = [];
    $scope.ccList = [];
    $scope.bccList = [];
    $scope.receiverDetailList = [];
    $scope.path = 'setups/mailreceiver/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getListUrl = $scope.path + 'getlist';
    baseService.init($scope.getListUrl, null, null, null, 'Name', 'Name');

    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.brands = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    $scope.Directmail = {
        Id: null,
        SourceType: 'Direct',
        Name: null,
        Email: null,
        Active: null
    }
    $scope.brand = {
        Id: null,
        Name: null,
        Remarks: null,
        CompanyGroupId: $window.companyGroupId,
        Active: true,
        MailReceipientType: 'Normal'
    };
    $scope.brandNew = Object.assign({}, $scope.brand);
    $scope.searchByList = [
        {
            'value': 'Name',
            'name': 'Name'
        },
        {
            'value': 'SenderName',
            'name': 'SenderName'
        }
    ];
    $scope.Get = function (id, index) {
        $scope.index = index;
        angular.copy($scope.brands[$scope.index], $scope.brand);
        angular.copy($scope.brand, $scope.brandNew);
        getTaggingUser();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    function getTaggingUser() {
        $http({
            method: 'GET',
            url: $scope.path + 'getTaggingUser?mailReceiverId=' + $scope.brandNew.Id
        }).then(function successCallback(response) {
            $scope.receiverDetailList = response.data;
            setGetDataToDetail($scope.receiverDetailList);
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    }
    function setGetDataToDetail(list) {
        $scope.toList = [];
        $scope.ccList = [];
        $scope.bccList = [];
        angular.forEach(list, function (item) {
            if (item.MailType === "To") {
                $scope.toList.push(item);
            } else if (item.MailType === "Cc") {
                $scope.ccList.push(item);
            } else if (item.MailType === "Bcc") {
                $scope.bccList.push(item);
            }
        });
    }
    function getDetailSaveData(tolist, cclist, bcclist) {
        $scope.receiverDetailList = [];
        if (tolist.length > 0)
            angular.forEach(tolist, function (item) {
                $scope.receiverDetailList.push(item);
            });
        if (cclist.length > 0)
            angular.forEach(cclist, function (item) {
                $scope.receiverDetailList.push(item);
            });
        if (bcclist.length > 0)
            angular.forEach(bcclist, function (item) {
                $scope.receiverDetailList.push(item);
            });
    }
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.emailReceiveForm.$valid) {
            getDetailSaveData($scope.toList, $scope.ccList, $scope.bccList);
            angular.copy($scope.brandNew, $scope.brand);
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'entity': $scope.brand,
                        'details': $scope.receiverDetailList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.brands.push(response.data.MailReceiver);
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }), function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === 'Update') {
                console.log($scope.receiverDetailList);
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: {
                        'entity': $scope.brand,
                        'details': $scope.receiverDetailList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1)
                            $scope.brands[$scope.index] = $scope.brand;
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.brandNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.brandNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.brands.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                } function errorCallback(response) {
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
        $scope.brand = {};
        $scope.brandNew = { Active: true };
        $scope.toList = [];
        $scope.ccList = [];
        $scope.bccList = [];
        $scope.receiverDetailList = [];
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #region ReturnToRequiredTab
    function reDirectToRequiredTab() {
        if ($scope.tab1.$invalid) $scope.setTab(1);
        else if ($scope.tab2.$invalid) $scope.setTab(2);
    }

    // #endregion

    //***********************************User ********************************************************//
    $scope.userTempList = [];
    $scope.selectchValue = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempUserList($scope.userTempList, data.Id) === false) {
                    $scope.userTempList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.userTempList.length; i++) {
                    if ($scope.userTempList[i].Id === data.Id) {
                        $scope.userTempList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    }
    function checkExistTempUserList(list, Id) {
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
    $rootScope.searchByUserList = [
        {
            'name': 'UserId',
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
            'name': 'AuthToken',
            'value': 'AuthToken'
        }
    ];
    $scope.valueData = '';
    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserId',
        searchBy: "UserId",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.userTaging = null;
    $scope.mailType = null;

    $scope.popUp = function (tag, mType) {
        $scope.userTempList = [];
        $scope.userTaging = tag;
        $scope.mailType = mType;
        $scope.popUpDataList = [];
        $scope.popUpUrl = 'securities/user/getlist';
        baseService.setCurrentPage('popUpDataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    for (var i = 0; i < $scope.popUpDataList.length; i++) {
                        $scope.popUpDataList[i].Flag = getActive($scope.userTempList, $scope.popUpDataList[i].Id);
                    }
                    for (var t = baseService.arrayLength($scope.popUpDataList) - 1; t >= 0; t--) {
                        //$scope.popUpDataList
                        if (baseService.valueCheckInList($scope.receiverList, 'UserId', $scope.popUpDataList[t].UserId))
                            $scope.popUpDataList.splice(t, 1);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    };

    $scope.checkEmail = function (data, event, index) {
        if (event.currentTarget.checked) {
            if (baseService.isUndefinedOrNull(data.Email))
                $scope.popUpDataList[index].Flag = false;
        }
    };
    $scope.selectByButton = function () {
        for (var t = 0; t < baseService.arrayLength($scope.userTempList); t++) {
            if ($scope.userTempList[t].Flag) {
                setTaging($scope.userTaging, $scope.userTempList[t]);
            }
        }
        $scope.closePopUp();
    };
    function setTaging(listName, ob) {
        if (checkExist($scope[listName], ob.Id) === false && checkExistAll(ob.Id) == false) {
            $scope[listName].push({
                Id: null
                , MailReceiverId: $scope.brandNew.Id
                , UserId: ob.Id
                , UserName: ob.UserId
                , EmployeeId: ob.EmployeeId
                , FullName: ob.UserName
                , Email: ob.Email
                , SourceType: 'User'
                , MailType: $scope.mailType
                , Active: ob.Active
            });
        }
        else {
            throw ShowResult("Already selected", 'failure', 'popUpId');
        }
    }
    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };
    $scope.addEmail = function () {
        if ($scope.Directmail.FullName != null && $scope.Directmail.Email != null) {
            $scope[$scope.userTaging].push($scope.Directmail);
            $scope.closePopUpAddMail();
        }
        else {
            throw ShowResult("Missing Name or Email", 'failure', 'popUpMail');
        }
    };
    $scope.addEmailPopUp = function (tag, mType) {
        //$scope.userTempList = [];
        $scope.userTaging = tag;
        $scope.mailType = mType;
        $scope.popUpDataList = [];
        $scope.Directmail = {
            Id: null,
            MailReceiverId: $scope.brandNew.Id,
            UserId: null,
            SourceType: 'Direct',
            FullName: null,
            MailType: $scope.mailType,
            Email: null,
            Active: true
        }

        angular.element(document.querySelector('#popUpMail')).modal('show');
    };
    $scope.closePopUpAddMail = function () {
        angular.element(document.querySelector('#popUpMail')).modal('hide');
    };
    //$scope.delPop = function (listname, index) {
    //    $scope.userTaging = listname;
    //    $scope.delIndex = index;
    //    $scope.message = 'Are you sure want to permanent delete';
    //    angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    //};
    //$scope.removeRow = function () {
    //    $scope[$scope.userTaging].splice($scope.delIndex, 1);
    //    $scope.delIndex = -1;
    //};

    $scope.delPop = function (listname, x, index) {
        $scope.userTaging = listname;
        $scope.delIndex = index;
        $scope.dId = x.Id;
        $scope.message = 'Are you sure to delete permanently?';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    $scope.removeRow = function () {
        if (baseService.isUndefinedOrNull($scope.dId)) {
            $scope[$scope.userTaging].splice($scope.delIndex, 1);
            $scope.delIndex = -1;
        }
        else {
            $http({
                method: 'POST',
                url: 'setups/mailreceiver/deletedetail?Id=' + $scope.dId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope[$scope.userTaging].splice($scope.delIndex, 1);
                    $scope.delIndex = -1;
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    //***********************************User ********************************************************//
    function checkExist(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].UserId === id) {
                return true;
                break;
            }
        }
        return false;
    }
    function checkExistAll(id) {
        if ($scope.mailType !== 'To') {
            for (var i = 0; i < $scope.toList.length; i++) {
                if ($scope.toList[i].UserId === id) {
                    return true;
                    break;
                }
            }
        }
        if ($scope.mailType !== 'Cc') {
            for (var i = 0; i < $scope.ccList.length; i++) {
                if ($scope.ccList[i].UserId === id) {
                    return true;
                    break;
                }
            }
        }
        if ($scope.mailType !== 'Bcc') {
            for (var i = 0; i < $scope.bccList.length; i++) {
                if ($scope.bccList[i].UserId === id) {
                    return true;
                    break;
                }
            }
        }

        return false;
    }
}