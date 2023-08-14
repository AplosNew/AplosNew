'use strict';
jobWorkLocationController.$inject = ['addressService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function jobWorkLocationController(addressService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Jobwork/Outsource Location";
    $scope.Action = 'Save';
    $scope.Action_Child = 'Add';
    $scope.path = 'Outsourcing/JobWorkLocation/';

    $scope.JobWorkLocation = {
        Id: null,
        PlantId: null,
        JobWorkLocationId: null,
        EntityId: null,
        LocationName: null,
        LocationCode: null,
        StoreLocationId: '',
        ResponsiblePerson1Id: null,
        ResponsiblePerson1Name: null,
        ResponsiblePerson2Id: null,
        ResponsiblePerson2Name: null,
        Remarks: '',
        IsActive: true
    };
    $scope.JobWorkLocationChild = {
        Id: '',
        ActivityName: ''
    };
    $scope.JobWorkLocationTypeList = [{ name: 'Value Added' }, { name: 'Transformation' }];

    //#region Partial View
    $controller("employeeBaseMultipleController", { $scope: $scope, $http: $http });
    //#endregion

    //#region  Partial View Call    
    $scope.closeResponsiblePersonPopUp = function () {
        if ($scope.responsiblePersonIndex !== -1) {
            var employee = $scope.employeeList[$scope.responsiblePersonIndex];
            if ($scope.responsiblePersonHiddenControlId === "EmployeeId1") {
                if (employee.SystemId === $scope.JobWorkLocation.ResponsiblePerson2Id) {
                    ShowResult("Responsible Person Already Exists..!", 'failure');
                }
                else {
                    $scope.JobWorkLocation.ResponsiblePerson1Name = employee.EmployeeName;
                    $scope.JobWorkLocation.ResponsiblePerson1Id = employee.SystemId;
                }
            }
            else if ($scope.responsiblePersonHiddenControlId === "EmployeeId2") {
                if (employee.SystemId === $scope.JobWorkLocation.ResponsiblePerson1Id) {
                    ShowResult("Responsible Person Already Exists..!", 'failure');
                }
                else {
                    $scope.JobWorkLocation.ResponsiblePerson2Name = employee.EmployeeName;
                    $scope.JobWorkLocation.ResponsiblePerson2Id = employee.SystemId;
                }
            }
        }
        $scope.hideResponsiblePersonPopUp();
    };
    $scope.hideResponsiblePersonPopUp = function () {
        angular.element(document.querySelector("#responsiblePersonPopUp")).modal("hide");
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.jobWorkLocation.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.path + "SaveData",
                    data: { 'saveData': $scope.JobWorkLocation, 'childData': $scope.gridChildDataList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.JobWorkLocation = response.data.Data;
                        $scope.getAllData();
                   //     $scope.Clear();
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
        $scope.JobWorkLocation.Id = $scope.selecteddata.Id;
        $scope.PopulateSelectedData($scope.JobWorkLocation.Id);
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
                $scope.JobWorkLocation.Id = response.data[0].Id;
                $scope.JobWorkLocation.PlantId = response.data[0].PlantId;
                $scope.getAllEntity();
                $scope.JobWorkLocation.EntityId = response.data[0].EntityId;
                $scope.JobWorkLocation.LocationName = response.data[0].LocationName;
                $scope.JobWorkLocation.LocationCode = response.data[0].LocationCode;
                $scope.JobWorkLocation.StoreLocationId = response.data[0].StoreLocationId;
                $scope.JobWorkLocation.ResponsiblePerson1Id = response.data[0].ResponsiblePerson1Id;
                $scope.JobWorkLocation.ResponsiblePerson1Name = response.data[0].ResponsiblePerson1Name;
                $scope.JobWorkLocation.ResponsiblePerson2Id = response.data[0].ResponsiblePerson2Id;
                $scope.JobWorkLocation.ResponsiblePerson2Name = response.data[0].ResponsiblePerson2Name;
                $scope.JobWorkLocation.IsActive = response.data[0].IsActive;
                $scope.JobWorkLocation.Remarks = response.data[0].Remarks;
                $scope.LoadAllSelectedJobLocationTab();

                $scope.Action = 'Update';

                if (!$rootScope.isCollapsed) {
                    $rootScope.toggle();
                }
            }
            else {
                ShowResult('No Data Found..!', 'failure');
            }
        });
    };

    // Delete
    $scope.DeleteSelectedData = function (Id) {
        var x = "#" + Id;
        var gridObj = $(x).data("ejGrid");
        $scope.selecteddata = gridObj.getSelectedRecords()[0];
        $scope.JobWorkLocation.Id = $scope.selecteddata.Id;

        $scope.message_confirmation = 'Are you sure want to Remove?';
        angular.element(document.querySelector('#confirmDeletePopUp')).modal('show');
    };

    $scope.removeRow = function () {
        try {
            $http({
                method: 'GET',
                url: $scope.path + "DeleteSelectedData?Id=" + $scope.JobWorkLocation.Id
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');

                    $scope.getAllData();
                    $scope.Action = 'Save';
                    $scope.Clear();
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
    $scope.plantList = [];
    $scope.getAllPlant = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetAllPlant'
        }).then(function successCallback(response) {
            $scope.plantList = response.data;
            for (var p = 0; p < $scope.plantList.length; p++) {
                if ($scope.plantList[p].Id == $window.plantId) {
                    $scope.JobWorkLocation.PlantId = $scope.plantList[p].Id;
                }

            }
        });
    };
    $scope.getAllPlant();

    $scope.getAllEntityStore = function () {
        $scope.getAllEntity();
    };

    $scope.entityList = [];
    $scope.getAllEntity = function () {
        if ($scope.JobWorkLocation.PlantId == null) {
            var PL = $window.plantId
            $scope.JobWorkLocation.PlantId = PL;
        }
        $http({
            method: 'GET',
            url: $scope.path + 'GetAllEntity?Id=' + $scope.JobWorkLocation.PlantId
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
            $scope.getAllStoreLocation();
        });
    };
    $scope.getAllEntity();

    $scope.storeLocationList = [];
    $scope.getAllStoreLocation = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetAllStoreLocation?Id=' + $scope.JobWorkLocation.PlantId
        }).then(function successCallback(response) {
            $scope.storeLocationList = response.data;
        });
    };

    $scope.gridActivityNameList = [];
    $scope.getAllActivityName = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetAllActivityName'
        }).then(function successCallback(response) {
            $scope.gridActivityNameList = response.data;
        });
    };
    //-------------------Save Child------------------------//

    $scope.EmpCatList = [];
    $scope.AddNewItem = function () {
        angular.element(document.querySelector("#EmpCatTabPopUp")).modal("show");
        $scope.getempcatTabData();

    }
    $scope.getempcatTabData = function () {
        $scope.EmpCatList = [];

        $http({
            method: 'POST',
            data: { MasterId: $scope.JobWorkLocation.Id },
            url: $scope.path + 'LoadJobActivityForSelection'
        }).then(function successCallback(response) {
            $scope.EmpCatList = response.data;
        });
    }

    $scope.SelectedEmpCategoryTabList = [];
    $scope.LoadAllSelectedJobLocationTab = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'LoadAllSelectedJobLocationTab?JobLocationMasterId=' + $scope.JobWorkLocation.Id
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
                throw 'Please select at least one Job Work Activity';
            }

            $http({
                method: 'POST',
                data: { JobLocationMasterId: $scope.JobWorkLocation.Id, JobActivtiyTabData: checkedData },
                url: $scope.path + 'SaveJobLocationChildTab'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.LoadAllSelectedJobLocationTab();
                }

            });
        }
        catch (e) {
            ShowResult(e, "failure");
        }


    }
    $scope.DelLocationChild = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DelLocationChild?Id=' + $scope.ChildTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.LoadAllSelectedJobLocationTab();
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
    $scope.Clear = function () {
        $scope.JobWorkLocation = {};
        $scope.JobWorkLocation.StoreLocationId = "";
        $scope.JobWorkLocation.Id = null;
        $scope.JobWorkLocation.Remarks = "";
        $scope.JobWorkLocation.IsActive = true;
        $scope.gridChildDataList = [];
        $scope.entityList = [];
        $scope.LoadAllSelectedJobLocationTab();
        $scope.getAllData();
        $scope.getAllPlant();
        $scope.getAllEntity();
        $scope.Action = 'Save';
    };

    $scope.Delete = function () {
        if ($scope.JobWorkLocation.Id === "") {
            ShowResult("No Data Found For Delete..!", 'failure');
        }
        else {
            $scope.message_confirmation = 'Are you sure want to Remove?';
            angular.element(document.querySelector('#confirmDeletePopUp')).modal('show');
        }
    };

    $scope.Clear();
    $scope.getAllActivityName();
    $scope.getAllData();
    $scope.getAllPlant();
    $scope.getAllData();
}