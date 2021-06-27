'use strict';
multipleIdCardController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function multipleIdCardController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {

    $rootScope.title = 'ID Card';
    $controller('employeeBaseController', { $scope: $scope, $http: $http });
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);
    $scope.paraModel = {
        FromDate: $filter('dateFiltering')(firstDay),
        ToDate: $filter('dateFiltering')(Date.now()),
        EmployeeId: null
    };




    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridPopUp").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmployeePopUpList.length; i++) {
                $scope.EmployeePopUpList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridPopUp").data("ejGrid");
        gridObj.refreshContent();
    };


    $scope.EmployeeList = [];
    $scope.GetEmployeeInformation = function () {
            $scope.searchbyonRoleEmpList = [];
            var parameters = { 'fromDate': $scope.paraModel.FromDate, 'toDate': $scope.paraModel.ToDate };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'Employees/EmployeeIdCard/GetAllEmployeeDataWithWorkType',
                data: parameters
            }).then(function successCallback(response) {
                if (response.data.length > 0) {
                    $scope.EmployeeList = response.data;
                    $scope.EmployeePopUpList = response.data;
                    //$scope.GetPopUpEmployee();
                }
            });
    };

    $scope.workTypeList = [];
    cboService.getEmployeeWorkTypeCbo(function (result) {
        $scope.workTypeList = result;
        if (baseService.arrayLength($scope.workTypeList)==1) {
            $scope.model.EmployeeWorkTypeId = $scope.workTypeList[0].Value;
        }
    });

    $scope.saveemployeedata = function () {
        var row = $filter('filter')($scope.EmployeePopUpList, { 'CheckBoxSelect': true });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            $scope.EmployeeList = row;
        }
        $scope.Back();
    }

    $scope.Back = function () {
        angular.element(document.querySelector('#EmpPopUp')).modal('hide');
    }

    $scope.EmployeePopUpList = [];
    $scope.GetPopUpEmployee = function () {
       
            var parameters = { 'fromDate': $scope.paraModel.FromDate, 'toDate': $scope.paraModel.ToDate };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'Employees/EmployeeIdCard/GetAllEmployeeDataWithWorkType',
                data: parameters
            }).then(function successCallback(response) {
                if (response.data.length > 0) {
                    $scope.EmployeePopUpList = response.data;
                }
            });
       
    };

    $scope.showEmployeeFilterScreen = function () {
        try {
            var gridObj = $("#GridPopUp").data("ejGrid");
            gridObj.clearFiltering();
            angular.element(document.querySelector('#EmpPopUp')).modal('show');

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.templateList = [];
    cboService.getTemplateCbo('IdCard', function (result) {
        $scope.templateList = result;
    });

    $scope.dataList = [];

    $scope.model = { Sequence: 0, IssueDate: null, EmployeeWorkTypeId: null, ExpiryDate: null, DOJ: null, EmpSystemId: null }

    $scope.rowcolor = function (args) {
        if (baseService.isUndefinedOrNull(args.data.Sequence))
            args.row.css("background-color", "#ff0000");
    }

    $scope.SaveIdIssue = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.model.IssueDate)) {
                throw "Issue Date is required.";
            }

            if (baseService.isUndefinedOrNull($scope.model.EmployeeWorkTypeId)) {
                throw "Work Type is required.";
            }

            //if (new Date($scope.model.IssueDate) < new Date($scope.model.DOJ)) {
            //    throw "IssueDate " + $scope.model.IssueDate + " can not less than DOJ " + $scope.model.DOJ + "";
            //}

            //if (baseService.arrayLength($scope.issueIdCardList) === 0) {
            //    if ((new Date($scope.model.IssueDate) < new Date($scope.model.DOJ)) || (new Date($scope.model.IssueDate) > new Date($scope.model.DOJ))) {
            //        throw "IssueDate " + $scope.model.IssueDate + " must be DOJ " + $scope.model.DOJ + "";
            //    }
            //}

            $http({
                method: "POST",
                url: 'employees/employeeinformation/createemployeeidcardissue',
                data: { "employeeIdCardIssue": $scope.model },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure", 'IdCardPopUpModel');
                }
                else {
                    ShowResult(response.data.Message, "success", 'IdCardPopUpModel');
                    $scope.GetSequence($scope.model.EmpSystemId);
                    $scope.GetEmployeeIssueCard($scope.model.EmpSystemId);
                    $scope.GetEmployeeInformation();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, "failure", 'IdCardPopUpModel');
            };
        } catch (e) {
            ShowResult(e, "failure", 'IdCardPopUpModel');
        }
    };

    $scope.GetSequence = function (empSystemId) {
        $http.get("Employees/EmployeeInformation/getautosequence?empSystemId=" + empSystemId)
            .then(function (response) {
                $scope.model.Sequence = response.data;
            });
    };

    $scope.GetIssue = function (data) {
        $scope.model = data;
    }

    $scope.GetEmployeeIssueCard = function (empSystemId) {
        $scope.issueIdCardList = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Employees/EmployeeInformation/GetIssueIdCardByEmployee?employeeId=' + empSystemId
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                for (var i = 0; i < response.data.length; i++) {
                    if (i == 0) {
                        $scope.issueIdCardList.push(response.data[i]);
                        $scope.model = response.data[i];

                        for (var i = 0; i < $scope.EmployeeList.length; i++) {
                            if ($scope.EmployeeList[i].EmpSystemId === empSystemId) {
                                $scope.EmployeeList[i].EmployeeWorkTypeId = $scope.model.EmployeeWorkTypeId;
                            }
                        }
                    }
                }
            }
        });
    }

    $scope.issueIdCardList = [];
    $scope.GetIDCard = function (obj) {
        $scope.issueIdCardList = [];

        try {
            $scope.model = { Sequence: 0, IssueDate: null, EmployeeWorkTypeId: null, ExpiryDate: null, DOJ: null, EmpSystemId: null }
            $scope.model.DOJ = obj.data.DOJ;
           // $scope.model.IssueDate = obj.data.DOJ;
            $scope.model.IssueDate = $filter('dateFiltering')(Date.now());
            $scope.model.EmpSystemId = obj.data.EmpSystemId;
            $http({
                method: "GET",
                dataType: 'JSON',
                url: 'Employees/EmployeeInformation/GetIssueIdCardByEmployee?employeeId=' + obj.data.EmpSystemId
            }).then(function successCallback(response) {
                if (baseService.arrayLength(response.data) > 0) {
                    for (var i = 0; i < response.data.length; i++) {
                        if (i == 0) {
                            $scope.issueIdCardList.push(response.data[i]);
                            $scope.model = response.data[i];
                        }
                    }
                }
                if (baseService.isUndefinedOrNull($scope.model.Sequence) || $scope.model.Sequence === 0) {
                    $scope.GetSequence(obj.data.EmpSystemId);
                }
            });

            angular.element(document.querySelector('#IdCardPopUpModel')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.IsCurrentIssueDate = false;
    $scope.DownLoadIdCard = function () {
        try {
            var gridObj = $("#Grid").ejGrid("instance");
            var filtereddata = gridObj.getFilteredRecords();
            if (filtereddata.length == 0) {
                filtereddata = $scope.EmployeeList;
            }
            $scope.EmployeeListNew = [];
            for (var i = 0; i < filtereddata.length; i++) {
                if ($scope.EmployeeListNew, filtereddata[i].EmpSystemId) {
                    $scope.EmployeeListNew.push(filtereddata[i].EmpSystemId);
                }
            }
            if (baseService.isUndefinedOrNull($scope.paraModel.tempId)) {
                throw "Select Language.";
            }
            if ($scope.EmployeeListNew.length <= 0) {
                throw "Select Employee.";
            }
            var ec = "";

            for (var i = 0; i < $scope.EmployeeListNew.length; i++) {
                for (var j = 0; j < $scope.EmployeeList.length; j++) {
                    if ($scope.EmployeeListNew[i] === $scope.EmployeeList[j].EmpSystemId) {
                        if (baseService.isUndefinedOrNull($scope.EmployeeList[i].EmployeeWorkTypeId)) {
                            if (baseService.isUndefinedOrNull(ec)) {
                                ec = $scope.EmployeeList[i].EmployeeCode;
                            } else {
                                ec += "," + $scope.EmployeeList[i].EmployeeCode;
                            }
                        }
                    }
                }
            }

            //$scope.dataList = ej.DataManager($scope.EmployeeList).executeLocal(ej.Query().select(["EmpSystemId", "EmployeeWorkTypeId", "EmployeeCode"]));
            //$scope.dataList = ej.DataManager($scope.EmployeeList).executeLocal(ej.Query().select(["EmpSystemId", "EmployeeWorkTypeId", "EmployeeCode"]));
            //for (var i = 0; i < $scope.dataList.length; i++) {
            //    if (baseService.isUndefinedOrNull($scope.dataList[i].EmployeeWorkTypeId)) {
            //        if (baseService.isUndefinedOrNull(ec)) {
            //            ec = $scope.dataList[i].EmployeeCode;
            //        } else {
            //            ec += "," + $scope.dataList[i].EmployeeCode;
            //        }
            //    }
            //}
            if (!baseService.isUndefinedOrNull(ec)) {
                throw "Input Employee Work Type for Employee Code: '" + ec + "'.";
            }
            var url = 'Employees/EmployeeIdCard/PrintMultipleIDCard?empId=' + $scope.EmployeeListNew + '&tempId=' + $scope.paraModel.tempId + '&issuDate=' + $scope.paraModel.IssueDate + '&dataList=' + $scope.dataList + '&IsCurrentIssueDate=' + $scope.IsCurrentIssueDate;
            $rootScope.report(url);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
}