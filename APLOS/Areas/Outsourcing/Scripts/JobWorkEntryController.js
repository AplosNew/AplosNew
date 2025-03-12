'use strict';
JobWorkEntryController.$inject = ['addressService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function JobWorkEntryController(addressService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Jobwork";
    $scope.Action = 'Save';
    $scope.path = 'Outsourcing/JobWorkEntry/';

    $scope.JobWorkItemModelTemp = {
        Id: null,
        CustomerInvoiceNo: null,
        CustomerInvoiceDate: null,
        CustomerName: null,
        Article: null,
        Lot: null,
        Shade: null,
        Quantity: null,
        MaterialValue: null,
        Sequence: null,
        TaxValue: null,
        GateEntryNo: null,
        Party: null,
        TotalValue: null,
        Remarks: null,
        Begs: null,
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
                
                if ($scope.JobWorkItem.GateEntryNo == null) {
                    throw 'Please select Gate Entry';          
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
                $scope.JobWorkItem.Sequence = response.data[0].Sequence;
                $scope.JobWorkItem.CustomerInvoiceNo = response.data[0].CustomerInvoiceNo;
                $scope.JobWorkItem.CustomerInvoiceDate = response.data[0].CustomerInvoiceDate;
                $scope.JobWorkItem.CustomerName = response.data[0].CustomerName;
                
                $scope.JobWorkItem.Article = response.data[0].Article;
                $scope.JobWorkItem.Lot = response.data[0].Lot;
                $scope.JobWorkItem.Shade = response.data[0].Shade;
                $scope.JobWorkItem.GateEntryNo = response.data[0].GateEntryNo;
                $scope.JobWorkItem.Quantity = response.data[0].Quantity;
                $scope.JobWorkItem.MaterialValue = response.data[0].MaterialValue;
                $scope.JobWorkItem.TaxValue = response.data[0].TaxValue;
                $scope.JobWorkItem.TotalValue = response.data[0].TotalValue;
                $scope.JobWorkItem.Remarks = response.data[0].Remarks;
                $scope.JobWorkItem.Begs = response.data[0].Begs;

                

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
        $scope.JobWorkItem.GateEntryNo = null;
        $scope.JobWorkItem.Party = null;
        $scope.JobWorkItem.GateEntryNo = null;

    };
    $scope.closeEmpResPersonPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setEmpData = function (obj) {

        var data = obj.data;
        $scope.JobWorkItem.GateEntryNo = data.GateEntryNo;
        $scope.JobWorkItem.GateEntryNo = data.GateEntryNo;
        $scope.JobWorkItem.Party = data.Party;
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