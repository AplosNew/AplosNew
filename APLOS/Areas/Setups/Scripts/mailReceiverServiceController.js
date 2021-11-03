'use strict';
mailReceiverServiceController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService'];
function mailReceiverServiceController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService) {
    $rootScope.title = "Mail Receiver Services";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.mailReceiverMappingList = [];
    $scope.receiverList = [];
    $scope.path = 'Setups/mailreceiver/';
    $scope.saveUrl = $scope.path + 'MailReceiverServiceMappingCreate';
    $scope.updateUrl = $scope.path + 'MailReceiverServiceMappingUpdate';
    $scope.deleteUrl = $scope.path + 'MailReceiverServiceMappingDelete/';
    $scope.getListUrl = $scope.path + 'MailReceiverServiceMappingGetList';

    baseService.init($scope.getListUrl, null, null, null, 'SenderName', 'SenderName');

    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.mailReceiverMappingList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    $scope.searchList = [
        {
            'name': 'Service Name',
            'value': 'ServiceName'
        },
        {
            'name': 'Mail Receiver Name',
            'value': 'MailReceiverName'
        },
        {
            'name': 'Sender Name',
            'value': 'SenderName'
        },
        {
            'name': 'Sender Email',
            'value': 'SenderEmail'
        }
    ];
    //**************CBO************/
    $scope.toList = [];
    $scope.ccList = [];
    $scope.bccList = [];
    //cboService.getCboPlant(function (result) {
    //    $scope.plantList = result;
    //});
    cboService.getEnumCbo("enum/GetMailServiceNameCbo", function (result) {
        $scope.serviceList = result;
    });
    cboService.getMailReceiverCbo(function (result) {
        $scope.receiverList = result;
    });
    $scope.brand = {
        Id: null,
        PlantId: null,
        MailReceiverId: null,
        MailReceiverName: null,
        ServiceName: null,
        IsSendMailIfEmptyData: null,
        SenderName: null,
        SenderEmail: null,
        Subject: null,
        MessageBody: null,
        Active: true
    };
    $scope.brandNew = Object.assign({}, $scope.brand);

    $scope.Get = function (id, index) {
        $scope.index = index;
        angular.copy($scope.mailReceiverMappingList[$scope.index], $scope.brand);
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

    $scope.getPlantList = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetPlant'
        }).then(function successCallback(response) {
            $scope.plantList = response.data;
            
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
        
    };
    $scope.getPlantList();

    $scope.setClickedRow = function (index) {
        $scope.selectedRow = index;
    };
    function setGetDataToDetail(list) {
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
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.emailReceiveServiceForm.$valid) {
            //$scope.brandNew.MailReceiverName = document.getElementById("altUOMId").options[document.getElementById('altUOMId').selectedIndex].text;
            $scope.brandNew.MailReceiverName = $.grep($scope.receiverList, function (item) {
                return item.Value === $scope.brandNew.MailReceiverId;
            })[0].Text;
            angular.copy($scope.brandNew, $scope.brand);
            if ($scope.brand.ServiceName == 'DailyAttendanceAudit')
            {
                if ($scope.brand.PlantId == null || $scope.brand.PlantId == '' || $scope.brand.PlantId == undefined)
                {
                    ShowResult("Please select Plant (Plant is Mandatory for [" + $scope.brand.ServiceName + "]) ", 'failure');
                    throw '';
                }
            }


            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'entity': $scope.brand
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        response.data.MailReceiver.MailReceiverName = $scope.brandNew.MailReceiverName;
                        $scope.mailReceiverMappingList.push(response.data.MailReceiver);
                        ClearFields();
                    }
                }), function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: {
                        'entity': $scope.brand,
                        'details': $scope.receiverList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1)
                            $scope.mailReceiverMappingList[$scope.index] = $scope.brand;
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
                    $scope.mailReceiverMappingList.splice($scope.index, 1);
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

    // #endregion

    //***********************************User ********************************************************//
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
        $scope.userTaging = tag;
        $scope.mailType = mType;
        $scope.popUpDataList = [];
        $scope.popUpUrl = 'Securities/user/getlist';
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
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
        for (var t = 0; t < baseService.arrayLength($scope.popUpDataList); t++) {
            if ($scope.popUpDataList[t].Flag) {
                setTaging($scope.userTaging, $scope.popUpDataList[t]);
            }
        }

        $scope.closePopUp();
    };
    function setTaging(listName, ob) {
        $scope[listName].push({
            Id: null
            , MailReceiverId: $scope.brandNew.Id
            , UserId: ob.Id
            , UserName: ob.UserId
            , EmployeeId: ob.EmployeeId
            , FullName: ob.UserId
            , Email: ob.Email
            , MailType: $scope.mailType
            , Active: ob.Active
        });
    }
    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };
    $scope.delPop = function (listname, index) {
        $scope.userTaging = listname;
        $scope.delIndex = index;
        $scope.message = 'Are you sure want to permanent delete';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };
    $scope.removeRow = function () {
        $scope[$scope.userTaging].splice($scope.delIndex, 1);
        $scope.delIndex = -1;
    };
    //***********************************User ********************************************************//
}