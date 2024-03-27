'use strict';
EmployeeSeperationSetupController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeSeperationSetupController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Seperation Setup';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Payrolls/EmployeeSeperationSetup/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
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
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {          
            $scope.ModelList = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetSequence();
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
        Active: true
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
        $scope.GetEmployeeCategory();
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

    $scope.ModelETTemp = {
        Id: null,
        EmployeeSeperationSetupId: null,
        EmployeeTypeId: null       
    };
    $scope.EmpCatNew = Object.assign({}, $scope.ModelETTemp);

    $scope.employeeTypeList = [];
    cboService.getCboEmployeeCategoryGroupByCompanyGroup(null, function (result) {
        $scope.employeeTypeList = result;
    });

    $scope.SaveEmployeeType = function () {
        $scope.EmpCatNew.EmployeeSeperationSetupId = $scope.ModelNew.Id;
        $http({
            method: 'POST',
            url: 'Payrolls/EmployeeSeperationSetup/CreateEmpSeperationEmployeeType',
            data: { 'data': $scope.EmpCatNew, 'masterId': $scope.ModelNew.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetEmployeeCategory();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };
    $scope.EmployeeCategoryList = [];
    $scope.GetEmployeeCategory = function () {
        $http({
            method: 'Get',
            url: "Payrolls/EmployeeSeperationSetup/GetEmpSeperationEmployeeTypeData?masterId=" + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EmployeeCategoryList = response.data;
        })
    }


    $scope.message_detailconfirmation = null;
    $scope.removeEmployeeCategory = function (obj) {
        $scope.EmployeeCat = obj;
        if (!baseService.isUndefinedOrNull($scope.EmployeeCat.Id))
            $scope.message_detailconfirmation = 'Are you sure want to delete permanently [ ' + $scope.EmployeeCat.EmployeeCategory + ' ]';
        angular.element(document.querySelector('#confirmEmployeeCategoryPopUp')).modal('show');
    }

    $scope.DeleteEmployeeCategory = function () {
        $http({
            method: 'POST',
            url: 'Payrolls/EmployeeSeperationSetup/DeleteEmployeeCategory?id=' + $scope.EmployeeCat.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetEmployeeCategory();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    $scope.DesignationGroupList = [];
    $scope.AddDesignationGroup = function () {
        $http({
            method: 'Get',
            url: "Payrolls/EmployeeSeperationSetup/GetDesignationGroupData",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DesignationGroupList = response.data;
            $scope.ShowResultCustom();
        })
    }

    $scope.ShowResultCustom = function (message, type) {
        $("#DesignationGroupPoUp").ejDialog("setTitle", "DesignationGroup");
        var eDialog = $("#DesignationGroupPoUp").data("ejDialog");
        eDialog.open();
        var gridObj = $("#GridDesignationGroup").data("ejGrid");
        gridObj.clearFiltering();  // clears all the filtering
    };

    $scope.SelectedDesignationGroupList = [];
    $scope.CloseDesignationGroup = function () {
        try {
            for (var i = 0; i < $scope.DesignationGroupList.length; i++) {
                if ($scope.DesignationGroupList[i].Active == true) {
                    if (checkExists($scope.SelectedDesignationGroupList, $scope.DesignationGroupList[i].Id) === false) {
                        var ob = {};
                        ob.Id = null;
                        ob.EmployeeSeperationSetupId = $scope.ModelNew.Id;
                        ob.DesignationGroupId = $scope.DesignationGroupList[i].Id;
                        ob.Sequence = $scope.DesignationGroupList[i].Sequence;
                        ob.Code = $scope.DesignationGroupList[i].Code;
                        ob.ShortName = $scope.DesignationGroupList[i].ShortName;
                        ob.StandardName = $scope.DesignationGroupList[i].StandardName;
                        ob.UserName = $scope.DesignationGroupList[i].UserName;
                        

                        $scope.SelectedDesignationGroupList.push(ob);
                        ob = {};
                    }
                }
            }
            $scope.SaveDesignationGroup();
            var eDialog = $("#DesignationGroupPoUp").data("ejDialog");
            eDialog.close();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].DesignationGroupId === id) {
                return true;
            }
        }
        return false;
    }


    $scope.GetSavedDesignationGroup = function () {
        $http({
            method: 'Get',
            url: "Payrolls/EmployeeSeperationSetup/GetEmpSepDesignationGroupData?masterId=" + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.SelectedDesignationGroupList = response.data;
        })
    }
    $scope.SaveDesignationGroup = function () {
        try {
            if (baseService.arrayLength($scope.SelectedDesignationGroupList) < 0) {
                throw "Select Designation Group.";
            }
            
            $http({
                method: 'POST',
                url: 'Payrolls/EmployeeSeperationSetup/GetEmpSepDesignationGroupData',
                data: { 'entities': $scope.SelectedDesignationGroupList, 'masterId': $scope.ModelNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetSavedDesignationGroup();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };







}