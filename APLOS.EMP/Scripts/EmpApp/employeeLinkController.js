EmployeeLinkController.$inject = ['$scope', '$http', '$location', "$rootScope", '$window', "$compile", 'baseService'];
function EmployeeLinkController($scope, $http, $location, $rootScope, $window, $compile, baseService) {
    $scope.title = 'Employee Initial Link';

    $scope.companyGroups = [];
    $http({
        method: 'GET',
        url: 'AplosEmpFieldTag/GetCompanyGroupCbo'
    }).then(function successCallback(response) {
        $scope.companyGroups = response.data;
    })
    $scope.empLink = {
        CompanyGroupId: null
        , CC: null
        , SenderName: null
        , SenderEmail: null
        , Subject: null
        , Message: null
    }
    $scope.empLinkNew = Object.assign({}, $scope.empLink);

    $scope.EmpEmailSend = function () {
        $scope.empLink = Object.assign({}, $scope.empLinkNew);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.empLinkForm.$valid) {
            $http({
                method: "post",
                url: 'employeelink/empemailsend',
                data: {
                    'empLink': $scope.empLink,
                    'employeeList': $scope.toList
                },
                dataType: "json"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                }
            }, function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            });
        }
    }
    $scope.ClearFields = function () {
        $scope.empLink = {};
        $scope.empLinkNew = {};
        tempList = [];
        $scope.popUpDataList = [];
        $scope.toList = [];
    }
    //***********************************To*****************************************************//
    var tempList = [];
    $scope.toList = [];
    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Name',
        searchBy: "Name",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.popUp = function () {
        $scope.popUpUrl = 'Employee/GetEmployeeListByCompanyGroup?companyGroupId=' + $scope.empLinkNew.CompanyGroupId;
        baseService.setCurrentPage('popUpDataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    for (var i = 0; i < $scope.popUpDataList.length; i++) {
                        $scope.popUpDataList[i].Flag = isTaken($scope.toList, $scope.popUpDataList[i].Id);
                    }
                    $scope.popUpParameters.total_count = result.total;
                    if (baseService.arrayLength($scope.popUpList) == 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                }, function () {
                    ShowResult(aplosMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    }
    function isTaken(list, value) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === value)
                return true;
        }
        return false;
    }
    $scope.selectByButton = function () {
        for (var i = 0; i < tempList.length; i++) {
            $scope.toList.push({
                Id: tempList[i].Id
                , Name: tempList[i].Name
                , Code: tempList[i].Code
                , CompanyId: tempList[i].CompanyId
                , InitialPIN: tempList[i].InitialPIN
                , Email: tempList[i].Email
            });
        }
        tempList = [];
        $scope.popUpDataList = [];
        angular.element(document.querySelector('#popUpId')).modal('hide');
    }
    $scope.selectEmployee = function (data, event, index) {
        try {
            if (data.Email === null) {
                $scope.popUpDataList[index].Flag = false;
                throw 'This employee has no email...........!';
            }
            if (event.currentTarget.checked)
                tempList.push(data);
            else {
                for (var i = 0; i < tempList.length; i++) {
                    if (tempList[i].Id)
                        tempList.splice(i, 1);
                }
            }
        } catch (e) {
            ShowResult(e, '', 'popUpId');
        }
    }
    //*********************************End To***************************************************//

    $scope.LogOff = function () {
        location.href = 'CPanel';
    }
};