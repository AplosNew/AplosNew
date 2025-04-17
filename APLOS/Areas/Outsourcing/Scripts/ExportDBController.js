'use strict';
ExportDBController.$inject = ['addressService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function ExportDBController(addressService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Export DB";
    $scope.Action = 'Save';
    $scope.path = 'Outsourcing/ExportDB/';

    $scope.JobWorkItemModelTemp = {
        Id: null,
        InvoiceNo: null,
        InvoiceDate: null,
        SBNumber: null,
        SBDate: null,
        PortCode: null,
        InvoiceValue: null,
        EXRate: null,
        FOBValueInr: null,
        RODTEP: null,
        DBKValue: null,
        IGSTAmount: null,
        CommPercentage: null,
        CommAmount: null,
        InsuranceAmount: null,
        Incoterms: null,
        Customer: null,
        CommDoller: null,
        FOBDoller: null,
        InsuranceDoller: null,
        Fright: null,
        FrightDoller: null,
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
                
                if ($scope.JobWorkItem.InvoiceNo == null) {
                    throw 'Please Invoice No Required';          
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
        $scope.RowId = x.data.InvoiceNo;
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
                $scope.JobWorkItem.InvoiceNo = response.data[0].InvoiceNo;
                $scope.JobWorkItem.InvoiceDate = response.data[0].InvoiceDate;
                $scope.JobWorkItem.SBNumber = response.data[0].SBNumber;
                $scope.JobWorkItem.SBDate = response.data[0].SBDate;
                
                $scope.JobWorkItem.PortCode = response.data[0].PortCode;
                $scope.JobWorkItem.InvoiceValue = response.data[0].InvoiceValue;
                $scope.JobWorkItem.EXRate = response.data[0].EXRate;
                $scope.JobWorkItem.FOBValueInr = response.data[0].FOBValueInr;
                $scope.JobWorkItem.RODTEP = response.data[0].RODTEP;
                $scope.JobWorkItem.DBKValue = response.data[0].DBKValue;
                $scope.JobWorkItem.IGSTAmount = response.data[0].IGSTAmount;
                $scope.JobWorkItem.CommPercentage = response.data[0].CommPercentage;
                $scope.JobWorkItem.CommAmount = response.data[0].CommAmount;
                $scope.JobWorkItem.InsuranceAmount = response.data[0].InsuranceAmount;
                $scope.JobWorkItem.Incoterms = response.data[0].Incoterms;
                $scope.JobWorkItem.CommDoller = response.data[0].CommDoller;
                $scope.JobWorkItem.FOBDoller = response.data[0].FOBDoller;
                $scope.JobWorkItem.InsuranceDoller = response.data[0].InsuranceDoller;
                $scope.JobWorkItem.Fright = response.data[0].Fright;
                $scope.JobWorkItem.FrightDoller = response.data[0].FrightDoller;
                $scope.JobWorkItem.Customer = response.data[0].Customer;

                

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