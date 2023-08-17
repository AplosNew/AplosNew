'use strict';
JobWorkItemController.$inject = ['addressService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function JobWorkItemController(addressService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Jobwork/Outsource Item";
    $scope.Action = 'Save';
    $scope.path = 'Outsourcing/JobWorkItem/';

    $scope.JobWorkItemModelTemp = {
        Id: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        UOMId: null,
        IsActive: true,
        ResponsiblePersonId: null,
        ResponsiblePersonName: null,
        Remarks: null,
        Sequence: null,
        MaterialMasterId: null,
        MaterialCode: null,
        MaterialName: null,
    };
    $scope.JobWorkItem = Object.assign({}, $scope.JobWorkItemModelTemp);

    $scope.uomList = [];

    //#region Partial View
    $controller("employeeBaseController", { $scope: $scope, $http: $http });
    //$controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    //$scope.materialType = ['Asset', 'Consumable', 'Spare', 'RawMaterial'];
    //#endregion

    //#region  Partial View Call    
    $scope.closeResponsiblePersonPopUp = function () {
        if ($scope.responsiblePersonIndex !== -1) {
            var employee = $scope.employeeList[$scope.responsiblePersonIndex];
            $scope.JobWorkItem.ResponsiblePersonName = employee.EmployeeName;
            $scope.JobWorkItem.ResponsiblePersonId = employee.SystemId;
        }
        $scope.hideResponsiblePersonPopUp();
    };
    $scope.hideResponsiblePersonPopUp = function () {
        angular.element(document.querySelector("#responsiblePersonPopUp")).modal("hide");
    };

    $scope.uomList = [];
    $scope.GetUOMList = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetUOMList'
        }).then(function successCallback(response) {
            $scope.uomList = response.data;
        });
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.jobWorkItem.$valid) {
                if ($scope.JobWorkItem.MaterialMasterId !== null) {
                    $scope.JobWorkItem.UOMId = null;
                }
                if ($scope.JobWorkItem.MaterialMasterId == null && $scope.JobWorkItem.UOMId == null) {
                    throw 'Please select UOM';          
                }

                $http({
                    method: 'POST',
                    url: $scope.path + "SaveData",
                    data: { 'saveData': $scope.JobWorkItem },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.Clear();
                        $scope.getAllData();
                        $scope.ItemSequenceNumber();
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

    $scope.gridDataList = [];
    $scope.getAllData = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetAllData'
        }).then(function successCallback(response) {
            $scope.gridDataList = response.data;
        });
    };

    $scope.ItemSequenceNumber = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetSequenceNumber'
        }).then(function successCallback(response) {
            $scope.JobWorkItem.Sequence = response.data[0].Sequence;
        });
    };

    $scope.recorddoubleclick = function ($event) {
        var x = $event;
        $scope.RowId = x.data.Id;
        $scope.PopulateSelectedDate($scope.RowId);
    };
    $scope.LoadSelectedData = function (Id) {
        var x = "#" + Id;
        var gridObj = $(x).data("ejGrid");
        $scope.selecteddata = gridObj.getSelectedRecords()[0];
        $scope.JobWorkItem.Id = $scope.selecteddata.Id;
        $scope.PopulateSelectedDate($scope.JobWorkItem.Id);
    };

    $scope.PopulateSelectedDate = function (Id) {
        $http({
            method: 'POST',
            url: $scope.path + 'GetSelectedData',
            data: {
                'Id': Id
            }
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.JobWorkItem.Id = response.data[0].Id;
                $scope.JobWorkItem.Code = response.data[0].Code;
                $scope.JobWorkItem.Sequence = response.data[0].Sequence;
                $scope.JobWorkItem.ShortName = response.data[0].ShortName;
                $scope.JobWorkItem.StandardName = response.data[0].StandardName;
                $scope.JobWorkItem.UserName = response.data[0].UserName;
                
                $scope.JobWorkItem.IsActive = response.data[0].IsActive;
                $scope.JobWorkItem.ResponsiblePersonId = response.data[0].ResponsiblePersonId;
                $scope.JobWorkItem.ResponsiblePersonName = response.data[0].ResponsiblePersonName;
                $scope.JobWorkItem.MaterialMasterId = response.data[0].MaterialMasterId;
                $scope.JobWorkItem.MaterialCode = response.data[0].MaterialCode;
                $scope.JobWorkItem.MaterialName = response.data[0].MaterialName;
                $scope.JobWorkItem.Remarks = response.data[0].Remarks;

                if ($scope.JobWorkItem.MaterialMasterId != null) {
                    $scope.getMatbaseUOM();
                }
                else {
                    $scope.JobWorkItem.UOMId = response.data[0].UOMId;
                }

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

    $scope.MatbaseUOM = [];
    $scope.getMatbaseUOM = function () {
        $scope.MatbaseUOM = [];
        $http({
            method: 'POST',
            data: { MatId: $scope.JobWorkItem.MaterialMasterId },
            url: $scope.path + 'getMatbaseUOM'
        }).then(function successCallback(response) {
            $scope.MatbaseUOM = response.data;
            if ($scope.MatbaseUOM.length > 0) {
                $scope.JobWorkItem.UOMId = $scope.MatbaseUOM[0].BaseUOMId;
            }
            
        });
    }

    $scope.DeleteSelectedData = function (Id) {
        var x = "#" + Id;
        var gridObj = $(x).data("ejGrid");
        $scope.selecteddata = gridObj.getSelectedRecords()[0];
        $scope.JobWorkItem.Id = $scope.selecteddata.Id;

        $scope.message = 'Are you sure want to Remove?';
        angular.element(document.querySelector('#confirmDeletePopUp')).modal('show');
    };

    $scope.removeRow = function () {
        try {
            $http({
                method: 'GET',
                url: $scope.path + "DeleteSelectedData?Id=" + $scope.JobWorkItem.Id
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
           
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.clearResponsiblePerson = function () {
        $scope.JobWorkItem.ResponsiblePersonName = "";
        $scope.JobWorkItem.ResponsiblePersonId = "";
    };

    $scope.Delete = function () {
        if ($scope.JobWorkItem.Id === "") {
            ShowResult("No Data Found For Delete..!", 'failure');
        }
        else {
            $scope.message_confirmation = 'Are you sure want to Remove?';
            angular.element(document.querySelector('#confirmDeletePopUp')).modal('show');
       
        }
    };

    $scope.Clear = function () {
        $scope.JobWorkItem = Object.assign({}, $scope.JobWorkItemModelTemp);
        $scope.Action = 'Save';
        $scope.ItemSequenceNumber();
    };

    $scope.Clear();
    $scope.GetUOMList();
    $scope.ItemSequenceNumber();
    $scope.getAllData();

    // #region field

    $scope.EmpResPersonList = [];
    $scope.MaterialMstPopUp = function () {
        angular.element(document.querySelector("#EmployeePopUpResPerson")).modal("show");
        $scope.getEmpDetailsData();

    }
    $scope.getEmpDetailsData = function () {
        $scope.EmpResPersonList = [];
        $http({
            method: 'POST',
            data: { Id: $scope.JobWorkItem.Id },
            url: $scope.path + 'LoadAllMaterialMstDetails'
        }).then(function successCallback(response) {
            $scope.EmpResPersonList = response.data;
        });
    }

    $scope.MaterialMstClear = function () {
        $scope.JobWorkItem.MaterialMasterId = null;
        $scope.JobWorkItem.MaterialName = null;
        $scope.JobWorkItem.MaterialCode = null;
        $scope.JobWorkItem.UOMId = null;

    };
    $scope.closeEmpResPersonPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setEmpData = function (obj) {

        var data = obj.data;
        $scope.JobWorkItem.MaterialCode = data.Code;
        $scope.JobWorkItem.MaterialMasterId = data.Id;
        $scope.JobWorkItem.MaterialName = data.MaterialName;
        $scope.JobWorkItem.UOMId = data.BaseUOMId;
        angular.element(document.querySelector('#EmployeePopUpResPerson')).modal('hide');
    };
    // # end region

    // #region field

    $scope.ApprovedByList = [];
    $scope.ResponsiblePersonListPopUp = function () {
        angular.element(document.querySelector("#ApprovedPopUp")).modal("show");
        $scope.getapprovedbyData();

    }
    $scope.getapprovedbyData = function () {
        $scope.ApprovedByList = [];

        $http({
            method: 'POST',
            data: { Id: $scope.JobWorkItem.Id },
            url: $scope.path + 'LoadResponsiblePersonDetails'
        }).then(function successCallback(response) {
            $scope.ApprovedByList = response.data;
        });
    }

    $scope.ResponsiblePersonClearDetails = function () {
        $scope.JobWorkItem.ResponsiblePersonId = null;
        $scope.JobWorkItem.ResponsiblePersonName = null;
   //     $scope.JobWorkItem.EmpCode = null;
        $scope.JobWorkItem.EmpStatus = null;
    };
    $scope.closeapprovedbyPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setApprovedByData = function (obj) {

        var data = obj.data;
    //    $scope.JobWorkItem.EmpCode = data.Code;
        $scope.JobWorkItem.ResponsiblePersonId = data.Id;
        $scope.JobWorkItem.ResponsiblePersonName = data.EmployeeName;
        angular.element(document.querySelector('#ApprovedPopUp')).modal('hide');
    };
    // # end region
}