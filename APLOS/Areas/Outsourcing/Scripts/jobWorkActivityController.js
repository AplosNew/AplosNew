'use strict';
jobWorkActivityController.$inject = ['addressService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function jobWorkActivityController(addressService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Jobwork/Outsource Activity";
    $scope.Action = 'Save';
    $scope.Action_Child = 'Add';
    $scope.path = 'Outsourcing/JobWorkActivity/';

    $scope.model = {
        Id: null,
        Code: null,
        ShortName: null,
        JobWorkItemId: null,
        StandardName: null,
        UserName: null,
        Type: null,
        IsOutsource: false,
        IsJobWork: false,
        IsActive: true,
        ResponsiblePersonId: '',
        ResponsiblePersonName: '',
        Remarks: '',
        Sequence: ''
    };
    $scope.JobWorkActivity = Object.assign({}, $scope.model);

    $scope.JobWorkActivityChild = {
        Id: '',
        ItemName: ''
    };

    $scope.jobWorkActivityTypeList = [{ name: 'Value Added' }, { name: 'Transformation' }];

    //#region Partial View
    $controller("employeeBaseController", { $scope: $scope, $http: $http });
    //#endregion

    //#region  Partial View Call    
    $scope.closeResponsiblePersonPopUp = function () {
        if ($scope.responsiblePersonIndex !== -1) {
            var employee = $scope.employeeList[$scope.responsiblePersonIndex];
            $scope.JobWorkActivity.ResponsiblePersonName = employee.EmployeeName;
            $scope.JobWorkActivity.ResponsiblePersonId = employee.SystemId;
        }
        $scope.hideResponsiblePersonPopUp();
    };
    $scope.hideResponsiblePersonPopUp = function () {
        angular.element(document.querySelector("#responsiblePersonPopUp")).modal("hide");
    };
    $scope.childGridDataList = [];
    $scope.Save = function () {
        angular.copy($scope.model, $scope.JobWorkActivity);
        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.jobWorkActivity.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.path + "SaveData",
                    data: { 'saveData': $scope.model },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
   
                        $scope.JobWorkActivity = response.data.Data;
                        $scope.getAllData();
                        ShowResult(response.data.Message, 'success');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.recorddoubleclick = function ($event) {
        var x = $event;
        $scope.RowId = x.data.Id;
        $scope.PopulateSelectedData($scope.RowId);
    };
    $scope.LoadSelectedData = function (Id) {
        var x = "#" + Id;
        var gridObj = $(x).data("ejGrid");
        $scope.selecteddata = gridObj.getSelectedRecords()[0];
        $scope.JobWorkActivity.Id = $scope.selecteddata.Id;
        $scope.PopulateSelectedData($scope.JobWorkActivity.Id);
    };
    $scope.PopulateSelectedData = function (Id) {
        $http({
            method: 'POST',
            url: $scope.path + 'GetSelectedData',
            data: {
                'Id': Id
            }
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.JobWorkActivity.Id = response.data[0].Id;
                $scope.JobWorkActivity.Sequence = response.data[0].Sequence;
                $scope.JobWorkActivity.Code = response.data[0].Code;
                $scope.JobWorkActivity.ShortName = response.data[0].ShortName;
                $scope.JobWorkActivity.StandardName = response.data[0].StandardName;
                $scope.JobWorkActivity.UserName = response.data[0].UserName;
                $scope.JobWorkActivity.Type = response.data[0].Type;
                $scope.JobWorkActivity.IsActive = response.data[0].IsActive;
                $scope.JobWorkActivity.ResponsiblePersonId = response.data[0].ResponsiblePersonId;
                $scope.JobWorkActivity.ResponsiblePersonName = response.data[0].ResponsiblePersonName;
                $scope.JobWorkActivity.Remarks = response.data[0].Remarks;

                //if (parseInt(response.data[0].Total) > 0)
                //    $("#ddlType").attr("disabled", true);
                //else
                //    $("#ddlType").removeAttr("disabled");

                $scope.Action = 'Update';

                if (!$rootScope.isCollapsed) {
                    $rootScope.toggle();
                }
                $scope.LoadAllSelectedJobActivityTab();
                $scope.GetDataToDisableType();
            }
            else {
                ShowResult('No Data Found..!', 'failure');
            }
        });
    };

 //   $scope.DisableType = false;
    $scope.GetDataToDisable = [];
    $scope.GetDataToDisableType = function () {
        $scope.GetDataToDisable = [];
        $http({
            method: 'GET',
            url: $scope.path + 'GetDataToDisable?JWActivityId=' + $scope.JobWorkActivity.Id + '&Type=' + $scope.JobWorkActivity.Type
        }).then(function successCallback(response) {
            $scope.GetDataToDisable = response.data;
            if ($scope.GetDataToDisable.length > 0) {
                $scope.DisableType = true;
            }
            else {
                $scope.DisableType = false;
            }
        });
    }

    $scope.DeleteSelectedData = function () {
        //var x = "#" + Id;
        //var gridObj = $(x).data("ejGrid");
        //$scope.selecteddata = gridObj.getSelectedRecords()[0];

        //$scope.JobWorkActivity.Id = $scope.JobWorkActivity.Id;

        $scope.message_confirmation = 'Are you sure want to Delete?';
        angular.element(document.querySelector('#confirmDeletePopUp')).modal('show');
    };

    $scope.removeRow = function () {
        try {
            $http({
                method: 'GET',
                url: $scope.path + "DeleteSelectedData?Id=" + $scope.JobWorkActivity.Id
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.getAllData();
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.gridDataList = [];

    $scope.getAllData = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetAllData'
        }).then(function successCallback(response) {
            $scope.gridDataList = response.data;
        });
    };
    //-------------------Child Data------------------------//    


    $scope.ItemSequenceNumber = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetSequenceNumber'
        }).then(function successCallback(response) {
            $scope.JobWorkActivity.Sequence = response.data[0].Sequence;
        });
    };

    /////////////////////////////////////////////////////////////////////////////////

    $scope.EmpCatList = [];
    $scope.AddNewItem = function () {
        angular.element(document.querySelector("#EmpCatTabPopUp")).modal("show");
        $scope.getempcatTabData();

    }
    $scope.getempcatTabData = function () {
        $scope.EmpCatList = [];

        $http({
            method: 'POST',
            data: { MasterId: $scope.JobWorkActivity.Id },
            url: $scope.path + 'LoadJobItemsForSelection'
        }).then(function successCallback(response) {
            $scope.EmpCatList = response.data;
        });
    }

    $scope.SelectedEmpCategoryTabList = [];
    $scope.LoadAllSelectedJobActivityTab = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'LoadAllSelectedJobActivtiyTab?JobWorkActivityMasterId=' + $scope.JobWorkActivity.Id
        }).then(function successCallback(response) {
            $scope.SelectedEmpCategoryTabList = response.data;
        });
    }

    //Save Function 
    $scope.EmpCategoryTabId = '';
    $scope.SaveEmpCatTab = function () {

        var checkedData = [];
        for (var i = 0; i < $scope.EmpCatList.length; i++) {
            if ($scope.EmpCatList[i].isSelected == true)
                checkedData.push($scope.EmpCatList[i]);
        }

        try {
            if (checkedData.length == 0) {
                throw 'Please select at least one Job Work Item';
            }

            $http({
                method: 'POST',
                data: { JobActivityMasterId: $scope.JobWorkActivity.Id, JobItemTabData: checkedData },
                url: $scope.path + 'SaveJobActivityChildTab'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.LoadAllSelectedJobActivityTab();
                }

            });
        }
        catch (e) {
            ShowResult(e, "failure");
        }


    }
    $scope.DelActivityChild = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DelActivityChild?Id=' + $scope.ChildTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.LoadAllSelectedJobActivityTab();
            }

        });
    }

    $scope.ConfirmDeleteEmpCategoryTab = function (Id) {
        $scope.ChildTabId = Id;
        angular.element(document.querySelector("#confirmDelEmpCatPopUp")).modal("show");
    }

    $scope.closeEmpCatTabPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }



    //-----------------------End--------------------------//    
    $scope.clearResponsiblePerson = function () {
        $scope.JobWorkActivity.ResponsiblePersonName = "";
        $scope.JobWorkActivity.ResponsiblePersonId = "";
    };

    $scope.Clear = function () {
        $scope.JobWorkActivity = {};
        $scope.JobWorkActivity.Id = null;
        $scope.JobWorkActivity.Remarks = '';
        $scope.JobWorkActivity.ResponsiblePersonId = '';
        $scope.JobWorkActivity.ResponsiblePersonName = '';
        $scope.JobWorkActivity.IsActive = true;
        $scope.JobWorkActivity.Sequence = '';
        $scope.gridChildDataList = [];
        $scope.Action = 'Save';
        $scope.ItemSequenceNumber();
        $scope.LoadAllSelectedJobActivityTab();
        $scope.getAllData();
        $scope.DisableType = false;
    //    $("#ddlType").removeAttr("disabled");
    };

    $scope.Clear();
    $scope.ItemSequenceNumber();
    $scope.getAllData();

    //$scope.DisableType = false;
    //$scope.Disable = function () {
    //    if ($scope.SelectedEmpCategoryTabList.length > 0) {
    //        $scope.DisableType = true;
    //    }
    //    else {
    //        $scope.DisableType = false;
    //    }
    //}
    //$scope.Disable();
}