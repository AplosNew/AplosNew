'use strict';
HRReportMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function HRReportMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'HR Report Master';
    $scope.Action = 'Save';
    $scope.ActionT2 = 'Save';
    $scope.ActionB = 'SaveBudgetCode' 
    $scope.ActionC = 'Save Responsible Person'
    $scope.ModelList = [];
    $scope.path = 'HumanResource/HRReportMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'GetSequence';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    // #region Lists
    $scope.EntityList = [];
    $scope.userMPList = [];
    $scope.BudgetList = [];
    $scope.UserGroupList = [];
    $scope.UserSubGroupList = [];
    $scope.GradeList = [];
    $scope.SelEmpList = [];
    // #endregion Lists

    // #region First Tab

    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
        // #endregion TAB CHANGE

    // #region Header Tab
    $scope.tabHeader = 1;
    $scope.setTabHeader = function (newTab) {
        $scope.tabHeader = newTab;
    };

    $scope.isSetHeader = function (tabNum) {
        return $scope.tabHeader === tabNum;
    };
    // #endregion Header Tab

    // #region ALL POP UPs
    // POP OPEN
    $scope.selectEmployee = function () {
        $scope.getEmployee($scope.ModelNew.Id);
        angular.element(document.querySelector('#EmployeePop')).modal('show');
    }

    

    // POP CLOSED
    $scope.closeEmpPopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
    }
    // #endregion ALL POP UPs

    // #region Get Fun

    // #region Get Sequence
  
    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

   // #endregion Get Sequence

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            //ClearFields(response.data.Sequence);
            $scope.GetSequence();
        });
    }
    $scope.getData();

    $scope.ChildMasterID = null;
    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.SelectedEmployeeId = args.data.EmpSystemId;
        $scope.Employee = args.data.Employee;
        $scope.Action = 'Update';
        //$scope.ActionB = 'UpdateBudgetCode'
        //$scope.ActionC = 'Update Responsible Person'
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
            //$scope.GetBudget($scope.ModelNew.Id);
            $scope.GetAllSavedBudgetCode(args.data.Id);
            $scope.GetSavedResponsiblePerson($scope.ModelNew.Id)
            //$scope.getEmployee($scope.ModelNew.Id);
        }

    };

   
    $scope.GetEntity = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetEntity",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EntityList = response.data;

        });
    }
    $scope.GetEntity();

    
    $scope.GetBudget = function () {
        // $scope.CheckedEntity = [];
        var DropDownEntityListObj = $("#entityId").data("ejDropDownList");
        var EntityId = DropDownEntityListObj.getSelectedValue();

        if (angular.isUndefinedOrNull(EntityId)) {
            for (var i = 0; i < DropDownEntityListObj.popupListItems.length; i++) {
                if (angular.isUndefinedOrNull(EntityId)) {
                    EntityId = + DropDownEntityListObj.popupListItems[i].Id;
                } else {
                    EntityId += ',' + DropDownEntityListObj.popupListItems[i].Id;
                }
            }
        }

        $http({
            method: 'POST',
            url: $scope.path + "GetBudgetCode",
            data: {
                'EntityId': EntityId,
                'id': $scope.ModelNew.Id
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.BudgetList = response.data;
           
        });
    }

    $scope.ViewAllBudgetCode = function () {
        var DropDownEntityListObj = $("#entityId").data("ejDropDownList");
        var EntityId = DropDownEntityListObj.getSelectedValue();
        if (angular.isUndefinedOrNull(EntityId)) {
            $http({
                method: 'POST',
                url: $scope.path + "ViewAllBudgetCode",
                data: {
                    'EntityId': EntityId,
                    'id': $scope.ModelNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.BudgetList = response.data;

            });
        }
        else {
            $scope.GetBudget();
        }
    }

    $scope.SavedBudgetList = [];
    $scope.GetAllSavedBudgetCode = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetAllSavedBudgetCode",
            data: { 'id': $scope.ModelNew.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.SavedBudgetList = response.data;
            
        });
    }

    
    $scope.GetUserGroup = function (masterId) {
        $http({
            method: 'POST',
            url: $scope.path + "GetUserGroup",
            data: { 'id': masterId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.UserGroupList = response.data;

        });
    }

    
    $scope.GetUserSubGroup = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetUserSubGroup",
            data: { 'userId': $scope.ob.UserGroupId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.UserSubGroupList = response.data;

        });
    }

   
    $scope.GetGrade = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetGrade",
            data: { 'userId': $scope.ob.UserGroupId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.GradeList = response.data;

        });
    }


    $scope.SavedEmployeeList = [];
    $scope.GetSavedResponsiblePerson = function (empSystemId) {
        $http({
            method: 'POST',
            url: $scope.path + "GetSavedResponsiblePerson",
            data: { 'headerId': $scope.ModelNew.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.SavedEmployeeList = response.data;
        })
    }
    // #endregion Get Fun
    
    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Category: null,
        SubCategory: null,
        StandardName: null,
        UserName: null,
        ShortName: null,
        Code: null,
        Active: true,
        Remarks: null,
        
        
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    // Header
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');

       /* if ($scope.ModelNewForm.$valid) {*/
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: {
                    'datas': $scope.ModelNew,
                    'Employee': $scope.SelectedEmployeeId,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ModelNew.Id = response.data.Data.Id;
                    //ClearFields(response.data.Sequence);
                    $scope.getData();
                    $scope.getEmployee($scope.ModelNew.Id);

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        /*}*/
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

    var checkedGroup = [];
    $scope.DeleteBudgetCode = function (x) {
        for (var i = 0; i < x.UserGroupList.length; i++) {
            if (x.UserGroupList[i].isSelected) {
                checkedGroup.push(x.UserGroupList[i])
            }
        }
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.path + 'DeleteBudgetCode',
                data: {
                    
                    'groupId': checkedGroup,
                    'bgtId': $scope.obj.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    angular.element(document.querySelector('#UserGroupPop2')).modal('hide');
                    $scope.GetAllSavedBudgetCode($scope.ModelNew.Id);
                    $scope.getData();

                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.Employee = null
        $scope.ModelNew = {
            Id: null,
            Sequence: 0,
            Category: null,
            SubCategory: null,
            StandardName: null,
            UserName: null,
            ShortName: null,
            Code: null,
            Active: true,
            Remarks: null,
            UserGroup: null,
            UserSubGroup: null,
        };
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;

        $scope.EmployeeIds = [];
        $scope.SelEmpList = [];

        for (var i = 0; i < $scope.EmployeeList.length; i++) {
            $scope.EmployeeList[i].isSelected = false;
        }

    }

    $scope.SelectedEmployeeId = null;
    $scope.EmployeeId = null;
    $scope.SelEmployeeInfoList = [];
    $scope.Employee = null;
    $scope.selectEmpDetail = function (e) {

        $scope.SelectedEmployeeId = e.data.SystemId;
        $scope.EmployeeId = e.data.EmployeeId;
        $scope.SelEmployeeInfoList = e.data;
        $scope.Employee = e.data.EmployeeName;
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
    }

    $scope.OpenUserGroupPopUp2 = function (data) {

        angular.element(document.querySelector('#UserGroupPop2')).modal('show');
        $scope.obj = data.data;
        $scope.GetUserGroup($scope.obj.Id);


    }

    $scope.obj = {};
    $scope.OpenUserGroupPopUp = function (data) {
        
        angular.element(document.querySelector('#UserGroupPop')).modal('show');
        $scope.obj = data.data;
        $scope.GetUserGroup();

 
    }

    $scope.CheckedUserGroupList = [];
    $scope.ClosePopupOnSelectAllField = function () {
        
        $scope.CheckedUserGroupList = [];
        for (var i = 0; i < $scope.UserGroupList.length; i++) {

            if ($scope.UserGroupList[i].isSelected) {
                $scope.CheckedUserGroupList.push($scope.UserGroupList[i]);
            }
            
        }
        $http({
            method: 'POST',
            url: $scope.path + 'Create',
            data: {
                'chkBgtList': $scope.obj,
                'usergroup': $scope.CheckedUserGroupList,
                'headerid': $scope.ModelNew.Id
            },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.obj.Id = response.data.Id;
                $scope.GetBudget($scope.ModelNew.Id);
                $scope.GetAllSavedBudgetCode($scope.ModelNew.Id);
                var gridObj = $("#bgtCodeGridId").data("ejGrid");
                angular.element(document.querySelector('#UserGroupPop')).modal('hide');
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
                $scope.UnchkOfCheckedItem();

            }
        });

        angular.element(document.querySelector('#UserGroupPop')).modal('hide');
        ShowResult("User Group Selected", 'success');

    }

    

    $scope.ActionB = 'SaveBudgetCode';
    $scope.CheckedBudgetCodeList = [];
    $scope.SaveBudgetCode = function () {
        $scope.CheckedBudgetCodeList = [];
        if ($scope.ActionB === 'SaveBudgetCode') {
            for (var i = 0; i < $scope.BudgetList.length; i++) {

                if ($scope.BudgetList[i].isSelected) {
                    $scope.CheckedBudgetCodeList.push($scope.BudgetList[i]);
                }
            }
            //if ($scope.ob.UserGroupId == null) {
            //    throw ShowResult("Please Select User Group");
            //}
            $http({
                method: 'POST',
                url: $scope.path + 'SaveBudgetCode',
                data: {
                    'chkBgtList': $scope.CheckedBudgetCodeList,
                    'usergroup': $scope.CheckedUserGroupList,
                    'headerid': $scope.ModelNew.Id
                },
                dataType: 'JSON',
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.UnchkOfCheckedItem();
                }
            });
        }
        else if ($scope.ActionB === 'UpdateBudgetCode') {
            $scope.UnCheckedBudgetCodeList = [];
            for (var i = 0; i < $scope.BudgetList.length; i++) {

                if ($scope.BudgetList[i].isSelected == false) {
                    $scope.UnCheckedBudgetCodeList.push($scope.BudgetList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.path + 'UpdateBudgetCode',
                data: {
                    'unchkBgtList': $scope.UnCheckedBudgetCodeList,
                    //'usersubgroup': $scope.UserSubGroupId,
                    'headerid': $scope.ModelNew.Id
                },
                dataType: 'JSON',
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.UnchkOfCheckedItem();
                }
            });
        }
    }
    // #region Responsible person Tab


    $scope.empearch = "";
    $scope.searchByEmp = "EmployeeCode"; $scope.search = "";
    $scope.searchEmpByList = [{ value: 'SystemID', name: "SystemID" }, { value: 'EmployeeCode', name: "Employee Code" }, { value: 'EmployeeName', name: "EmployeeName" }];


    $scope.EmployeeList = [];
    $scope.getEmployee = function (headerId) {
        $http({
            method: 'POST',
            url: $scope.path + "getEmployee",
            data: { column: $scope.searchByEmp, value: $scope.empearch, 'headerid': headerId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EmployeeList = response.data;
        });
    }

    //$scope.getEmployee();

    $scope.Save_One_or_MultipleResPers = function () {
        $scope.CheckedResponsiblePersonList = [];
        if ($scope.ActionC === 'Save Responsible Person') {
            for (var i = 0; i < $scope.EmployeeList.length; i++) {

                if ($scope.EmployeeList[i].isSelected) {
                    $scope.CheckedResponsiblePersonList.push($scope.EmployeeList[i]);
                }
            }
           
            $http({
                method: 'POST',
                url: $scope.path + 'Save_One_or_MultipleResPers',
                data: {
                    'chkRespersonList': $scope.CheckedResponsiblePersonList,
                    
                    'headerid': $scope.ModelNew.Id
                },
                dataType: 'JSON',
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ModelNew.Id = $scope.ModelNew.Id;
                    for (var i = 0; i < $scope.CheckedResponsiblePersonList.length; i++) {

                        if ($scope.CheckedResponsiblePersonList[i].isSelected) {
                            $scope.CheckedResponsiblePersonList[i].isSelected = false;
                        }
                    }
                    $scope.getEmployee($scope.ModelNew.Id);
                    $scope.GetSavedResponsiblePerson($scope.ModelNew.Id);
                    angular.element(document.querySelector('#EmployeePop')).modal('hide');
                }
            });
        }
       
    }

    $scope.UpdateResponsiblePerson = function () {
        $scope.CheckedResponsiblePersonList = [];
        for (var i = 0; i < $scope.SavedEmployeeList.length; i++) {

            if ($scope.SavedEmployeeList[i].isSelected == false) {
                $scope.CheckedResponsiblePersonList.push($scope.SavedEmployeeList[i]);
            }
        }

        $http({
            method: 'POST',
            url: $scope.path + 'Save_One_or_MultipleResPers',
            data: {
                'chkRespersonList': $scope.CheckedResponsiblePersonList,

                'headerid': $scope.ModelNew.Id
            },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.ModelNew.Id = $scope.ModelNew.Id;
                //for (var i = 0; i < $scope.CheckedResponsiblePersonList.length; i++) {

                //    if ($scope.CheckedResponsiblePersonList[i].isSelected) {
                //        $scope.CheckedResponsiblePersonList[i].isSelected = false;
                //    }
                //}
                $scope.getEmployee($scope.ModelNew.Id);
                $scope.GetSavedResponsiblePerson($scope.ModelNew.Id);
                
            }
        });
    }
    
    $scope.DeleteResponsiblePerson = function () {
        $scope.CheckedResponsiblePersonList = [];
        for (var i = 0; i < $scope.SavedEmployeeList.length; i++) {

            if ($scope.SavedEmployeeList[i].isSelected == false) {
                $scope.CheckedResponsiblePersonList.push($scope.SavedEmployeeList[i]);
            }
        }

        
            $http({
                method: 'POST',
                url: 'HumanResource/HRReportMaster/DeleteResponsiblePerson',
                data: { 'data': $scope.CheckedResponsiblePersonList},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');  
                    $scope.getEmployee($scope.ModelNew.Id);
                    $scope.GetSavedResponsiblePerson($scope.ModelNew.Id);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
       
    };

    // #endregion Responsible person Tab

    $scope.UnchkOfCheckedItem = function () {
        for (var i = 0; i < $scope.CheckedBudgetCodeList.length; i++) {

            if ($scope.CheckedBudgetCodeList[i].isSelected) {
                $scope.CheckedBudgetCodeList[i].isSelected = false;
            }
        }
    }

    
    // #endregion First Tab

    // #region 2nd Tab
    $scope.ModelTempB = {
        Id: null,        
        UserGroup: null,
        UserSubGroup: null,
        Grade: null
    };
    $scope.ModelNewB = Object.assign({}, $scope.ModelTempB);

    $scope.GetB = function (args) {

        $scope.ModelNewB = Object.assign({}, args.data);
        $scope.ActionT2 = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
           
        }
    };

    $scope.ModelListB = [];
    $scope.getDataB = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetListB",

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelListB = response.data;
            ClearFieldsB();
            
        });
    }
    $scope.getDataB();

    $scope.SaveB = function () {
        $scope.$broadcast('show-errors-check-validity');

        if ($scope.ModelNewBForm.$valid) {
            $http({
                method: 'POST',
                url: 'HumanResource/HRReportMaster/SaveB',
                data: {
                    'datas': $scope.ModelNewB,
                   
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsB();
                    $scope.getDataB();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DeleteB = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: 'HumanResource/HRReportMaster/DeleteB' + $scope.ModelNewB.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsB();
                    $scope.getDataB();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.ClearB = function () {
        ClearFieldsB();
        return true;
    };

    function ClearFieldsB() {
        $scope.ActionT2 = 'Save';
        $scope.Employee = null
        $scope.ModelNew = {
            Id: null,
            UserGroup: null,
            UserSubGroup: null,
            Grade: null
        };
        $scope.ModelNewB = Object.assign({}, $scope.ModelTempB);
    }
      
    // #endregion 2nd Tab
    
}