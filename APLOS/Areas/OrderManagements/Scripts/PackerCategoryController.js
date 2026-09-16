'use strict';
PackerCategoryController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function PackerCategoryController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Packer Category';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'OrderManagements/ProductionOrder/';
    $scope.getListUrl = $scope.path + 'GetPackerCategoryList';
    $scope.getSeqUrl = $scope.path + 'GetPackerCategoryAutoSequence';
    $scope.saveUrl = $scope.path + 'CreatePackerCategory';
    $scope.deleteUrl = $scope.path + 'DeletePackerCategory/';
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.getListUrl,
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        PackingPoint: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.getSavedEmpData();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
    }

    $scope.popUpDataList = [];
    $scope.getEmpPopUpData = function () {
        try {
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'employees/authorizationconfig/getallemployeedata'

            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });
            angular.element(document.querySelector('#popUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };  

    $scope.closePopUp = function () {
        MakeData();
        $scope.SavePackerEmployee();
        angular.element(document.querySelector('#popUp')).modal('hide');
    };

    // #region checkbox all

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
            for (var i = 0; i < $scope.popUpDataList.length; i++) {
                $scope.popUpDataList[i].Flag = ChkOrUnchk;
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

    // #endregion checkbox all

    $scope.packerEmployeeList = [];
    function MakeData() {
        for (var i = 0; i < $scope.popUpDataList.length; i++) {
            if ($scope.popUpDataList[i].Flag == true) {
                if (checkExists($scope.packerEmployeeList, $scope.popUpDataList[i].SystemID) === false) {
                    var ob = {};
                    ob.Id = -(Math.floor(Math.random() * 100) + 1);
                    ob.PackerCategoryId = $scope.ModelNew.Id;
                    ob.EmpSystemId = $scope.popUpDataList[i].SystemId;
                    ob.EmployeeCode = $scope.popUpDataList[i].EmployeeCode;
                    ob.EmployeeName = $scope.popUpDataList[i].EmployeeName;
                    ob.LegalDesignation = $scope.popUpDataList[i].LegalDesignation;
                    ob.Department = $scope.popUpDataList[i].Department;
                    ob.Company = $scope.popUpDataList[i].Company;
                    ob.Plant = $scope.popUpDataList[i].Plant;
                    ob.Section = $scope.popUpDataList[i].Section;
                    ob.SubSection = $scope.popUpDataList[i].SubSection;
                    ob.Line = $scope.popUpDataList[i].Line;

                    $scope.packerEmployeeList.push(ob);
                }
            }
        }

    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmpSystemId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.SavePackerEmployee = function () {
        try {
            if (baseService.arrayLength($scope.packerEmployeeList) < 0) {
                throw "Select Employee.";
            }
            $http({
                method: 'POST',
                url: 'OrderManagements/ProductionOrder/SavePackerEmployee',
                data: { 'data': $scope.packerEmployeeList},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSavedEmpData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.packerEmployeeList = [];
    $scope.getSavedEmpData = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductionOrder/getpackeremployee?masterId=' + $scope.ModelNew.Id
        }).then(function successCallback(response) {
            $scope.packerEmployeeList = response.data;

        });

    };

    $scope.message_confirmation = "";
    $scope.removeEmp = function (data) {
        $scope.runobj = data.data;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + $scope.runobj.EmployeeCode + ' ]';
        angular.element(document.querySelector('#confirmRunDelPopUp')).modal('show');
    };
    $scope.DeleteEmployee = function () {
        if (!baseService.isUndefinedOrNull($scope.runobj.Id)) {
            $http({
                method: 'POST',
                url: 'OrderManagements/ProductionOrder/DeleteEmployee?id=' + $scope.runobj.Id
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSavedEmpData();
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }

    };

}