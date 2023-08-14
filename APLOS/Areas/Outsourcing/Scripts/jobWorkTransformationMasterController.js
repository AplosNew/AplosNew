'use strict';
jobWorkTransformationMasterController.$inject = ['addressService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function jobWorkTransformationMasterController(addressService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Jobwork/Outsource Transformation Master";
    $scope.Action = 'Save';
    $scope.tab = 1;
    $scope.ActionMaterialInput_Child = 'Add';
    $scope.ActionByProduct_Child = 'Add';
    $scope.path = 'Outsourcing/JobWorkTransformationMaster/';
    $controller("currencyBaseController", { $scope: $scope, $http: $http });

    $scope.Transformation = {
        Id: '',
        JobWorkActivityId: null,
        JobWorkActivityChildId: null,
        UOM: null,
        RateApplicable: null,
        CurrencyId: null,
        MinRate: null,
        MaxRate: null,
        CycleTime: null,
        ResponsiblePerson: null,
        ResponsiblePersonId: null,
        ByProductApplicable: null,
        Remarks: '',
        MaterialCode: null,
        MaterialName: null,
        MaterialMasterId: null,
        ServiceId: null,
    };

    $scope.MaterialInput = {
        JobWorkTransformationMasterId: null,
        JobWorkItemId: '',
        ItemSpecification: null,
        UOM: null,
        StandardQty: null,
        Rejection: null,
        ValueLoss: null,
        GrossQty: null,
        StandardRate: '',
        ResponsiblePerson: '',
        ResponsiblePersonId: null,
        Remarksss: '',
    };
    $scope.ByProduct = {
        JobWorkTransformationMasterId: null,
        JobWorkItemId: '',
        ItemSpecification: null,
        StandardQty: null,
        PercentageOfInput: null,
        CurrencyId: null,
        StandardRate: null,
        ResponsiblePerson: '',
        ResponsiblePersonId: null,
        Remarks: '',
        //UOM: null,
        //Material: null,
    };
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.rateApplicableList = [{ name: 'Input' }, { name: 'Output' }];
    $scope.byProductApplicable = [{ name: 'Yes' }, { name: 'No' }];
    $controller("employeeBaseMultipleController", { $scope: $scope, $http: $http });

    $scope.create = function (args) {
        $("#checkBox").ejCheckBox({
            change: function (args) {
                var obj = $("#ddlProcess").ejDropDownList("instance");
                if (args.isChecked) obj.checkAll();
                else obj.uncheckAll();
            },
            text: "Select All",
            cssClass: "ddlSelectAllCheckBox"
        });
    };

    $scope.closeResponsiblePersonPopUp = function () {
        if ($scope.responsiblePersonIndex !== -1) {
            var employee = $scope.employeeList[$scope.responsiblePersonIndex];
            if ($scope.responsiblePersonHiddenControlId === "Master") {
                $scope.Transformation.ResponsiblePerson = employee.EmployeeName;
                $scope.Transformation.ResponsiblePersonId = employee.SystemId;
            }
            else if ($scope.responsiblePersonHiddenControlId === "Child1") {
                $scope.GridIndex = $scope.responsiblePersonTextControlId;
                $scope.materialInputList[$scope.GridIndex].ResponsiblePerson = employee.EmployeeName;
                $scope.materialInputList[$scope.GridIndex].ResponsiblePersonId = employee.SystemId;
            }
            else if ($scope.responsiblePersonHiddenControlId === "Child2") {
                $scope.GridIndex = $scope.responsiblePersonTextControlId;
                $scope.byProductList[$scope.GridIndex].ResponsiblePerson = employee.EmployeeName;
                $scope.byProductList[$scope.GridIndex].ResponsiblePersonId = employee.SystemId;
            }
            else if ($scope.responsiblePersonHiddenControlId === "MaterialInput") {
                $scope.MaterialInput.MaterialInputResponsiblePerson = employee.EmployeeName;
                $scope.MaterialInput.MaterialInputResponsiblePersonId = employee.SystemId;
            }
            else if ($scope.responsiblePersonHiddenControlId === "ByProduct") {
                $scope.ByProduct.ByProductResponsiblePerson = employee.EmployeeName;
                $scope.ByProduct.ByProductResponsiblePersonId = employee.SystemId;
            }
        }
        $scope.hideResponsiblePersonPopUp();
    };
    $scope.hideResponsiblePersonPopUp = function () {
        angular.element(document.querySelector("#responsiblePersonPopUp")).modal("hide");
    };

    $scope.Save = function () {
        $scope.ValidateMaxRate();
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.jobWorkTransformationMasterform.$valid ) {

                var DropDownListObj = $("#ddlProcess").data("ejDropDownList");
                var selectedProcessList = DropDownListObj.getSelectedValue();
                var arrProcessList = selectedProcessList.split(",");

                $scope.selProcessList = [];
                arrProcessList.forEach(function (item) {
                    if (item !== "") {
                        $scope.selProcessList.push({
                            ProcessId: item
                        });
                    }
                });

                if (selectedProcessList === "") {
                    ShowResult("Please Select Process", 'failure');
                    return false;
                }

                if ($scope.materialInputList.length === 0) {
                    ShowResult("No Material Input Data Found..!", 'failure');
                    return false;
                }

                if ($scope.Transformation.ByProductApplicable === "Yes") {
                    if ($scope.byProductList.length === 0) {
                        ShowResult("No By Product Data Found..!", 'failure');
                        return false;
                    }
                }

                $http({
                    method: 'POST',
                    url: $scope.path + "SaveData",
                    data: { 'saveData': $scope.Transformation, 'childData': $scope.selProcessList, 'materialInput': $scope.materialInputList, 'byProduct': $scope.byProductList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.getAllData();
                        $scope.Clear();
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
    $scope.SetShowHideTab = function () {
        //alert($scope.Transformation.ByProductApplicable);
        if ($scope.Transformation.ByProductApplicable === "No")
            $scope.tab = 1;
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
        $scope.Transformation.Id = $scope.selecteddata.Id;
        $scope.PopulateSelectedData($scope.Transformation.Id);
    };

    $scope.ShowActivity = true;
    $scope.LabelActivity = false;

    $scope.PopulateSelectedData = function (Id) {
        $http({
            method: 'POST',
            url: $scope.path + 'GetSelectedData',
            data: {
                'Id': Id
            }
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.Transformation.Id = response.data[0].Id;
                $scope.ShowActivity = false;
                $scope.LabelActivity = true;
                $scope.Transformation.ActivityName = response.data[0].Activity;
                $scope.Transformation.JobWorkActivityId = response.data[0].JobWorkActivityId;
                $scope.getActivityChildItems();
                $scope.Transformation.JobWorkActivityChildId = response.data[0].JobWorkActivityChildId;
           //     $scope.Transformation.UOM = response.data[0].UOM;
                $scope.Transformation.RateApplicable = response.data[0].RateApplicable;
                $scope.Transformation.CurrencyId = response.data[0].CurrencyId;
                $scope.Transformation.MinRate = response.data[0].MinRate;
                $scope.Transformation.MaxRate = response.data[0].MaxRate;
                $scope.Transformation.CycleTime = response.data[0].CycleTime;
                $scope.Transformation.ResponsiblePerson = response.data[0].ResponsiblePerson;
                $scope.Transformation.ResponsiblePersonId = response.data[0].ResponsiblePersonId;
                $scope.Transformation.ByProductApplicable = response.data[0].ByProductApplicable;
                $scope.Transformation.Remarks = response.data[0].Remarks;
                $scope.Transformation.ServiceId = response.data[0].ServiceId;

                $scope.getJobWorkItemUOM();

                $scope.getSelectedProcessData();
                $scope.getAllMaterialInput();
                $scope.getAllByProduct();

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

    $scope.getSelectedProcessData = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetSelectedProcessData?Id=' + $scope.Transformation.Id
        }).then(function successCallback(response) {
            var DropDownListObj = $("#ddlProcess").data("ejDropDownList");
            DropDownListObj.uncheckAll();
            for (var j = 0; j < response.data.length; j++) {
                DropDownListObj.selectItemByValue(response.data[j].ProcessId);
            }
        });
    };

    $scope.DeleteSelectedData = function () {
        //var x = "#" + Id;
        //var gridObj = $(x).data("ejGrid");
        //$scope.selecteddata = gridObj.getSelectedRecords()[0];
        //$scope.Transformation.Id = $scope.selecteddata.Id;

        $scope.message_confirmation = 'Are you sure want to Delete?';
        angular.element(document.querySelector('#confirmDeletePopUp')).modal('show');
    };

    $scope.removeRow = function () {
        try {
            $http({
                method: 'GET',
                url: $scope.path + "DeleteSelectedData?Id=" + $scope.Transformation.Id
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');

                    $scope.getAllData();
                    $scope.Clear();
                    $scope.Action = 'Save';
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.removeRowModalMaterial = function (index, data) {
        try {
            $scope.Index = index;
            $scope.Id = data.Id;
            $scope.message = 'Are you sure want to delete this data....';
            angular.element(document.querySelector('#removerMaterialPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.removeMaterialChildRow = function () {
        if (!baseService.isUndefinedOrNull($scope.Index) && $scope.Id == "") {
            $scope.materialInputList.splice($scope.Index, 1);
        }
        else {
            $http({
                method: 'GET',
                url: $scope.path + 'DeleteMaterialChildData?Id=' + $scope.Id
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.materialInputList.splice($scope.Index, 1);
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };

    $scope.removeRowModalProduct = function (index, data) {
        try {
            $scope.Index = index;
            $scope.Id = data.Id;
            $scope.message = 'Are you sure want to delete this data....';
            angular.element(document.querySelector('#removerProductPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.removeProductChildRow = function () {
        if (!baseService.isUndefinedOrNull($scope.Index) && $scope.Id == "") {
            $scope.byProductList.splice($scope.Index, 1);
        }
        else {
            $http({
                method: 'GET',
                url: $scope.path + 'DeleteProductChildData?Id=' + $scope.Id
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.byProductList.splice($scope.Index, 1);
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
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

    $scope.currencyList = [];
    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = [];
        $scope.currencyList = result;
    });

    $scope.jobWorkActivityList = [];
    $scope.getAllActivityUserName = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetAllActivityUserName'
        }).then(function successCallback(response) {
            $scope.jobWorkActivityList = response.data;
        });
    };

    $scope.processList = [];
    $scope.getAllProcessName = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetAllProcessName'
        }).then(function successCallback(response) {
            $scope.processList = response.data;
        });
    };

    $scope.materialUOMList = [];
    $scope.getJobWorkItemUOM = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getJobWorkItemUOM?Id=' + $scope.Transformation.JobWorkActivityChildId
        }).then(function successCallback(response) {
            $scope.materialUOMList = response.data;
            if ($scope.materialUOMList.length > 0) {
                if ($scope.materialUOMList[0].MaterialMasterId != null) {
                    $scope.Transformation.MaterialCode = $scope.materialUOMList[0].MaterialCode;
                    $scope.Transformation.MaterialName = $scope.materialUOMList[0].Material;
                    $scope.Transformation.UOM = $scope.materialUOMList[0].MMUnit;
                }
                else {
                    $scope.Transformation.UOM = $scope.materialUOMList[0].JWIUnit;
                    $scope.Transformation.MaterialCode = null;
                    $scope.Transformation.MaterialName = null;
                }
            }
            
        });
    };

    $scope.activityChildItemsList = [];
    $scope.getActivityChildItems = function () {
        $scope.activityChildItemsList = [];
        $scope.Transformation.UOM = null;
        $scope.Transformation.MaterialCode = null;
        $scope.Transformation.MaterialName = null;
        $http({
            method: 'GET',
            url: $scope.path + 'GetActivityChildItems?Id=' + $scope.Transformation.JobWorkActivityId
        }).then(function successCallback(response) {
            $scope.activityChildItemsList = response.data;
        });
    };

    $scope.materialInputList = [];
    $scope.AddNewRow = function () {
        $scope.ActionMaterialInput_Child = 'Add';
        angular.element(document.querySelector("#modalMaterialInput")).modal("toggle");
    };
    $scope.AddNewMaterialInputRow = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.materialInputForm.$valid) {

            $scope.materialInputList.push({
                Id: '',
                JobWorkItemId: $scope.MaterialInput.JobWorkItemId,
                ItemSpecification: $scope.MaterialInput.ItemSpecification,
                UOM: $scope.MaterialInput.UOM,
                NetConsumption: $scope.MaterialInput.NetConsumption,
                Rejection: $scope.MaterialInput.Rejection,
                ValueLoss: $scope.MaterialInput.ValueLoss,
                GrossConsumption: $scope.MaterialInput.GrossConsumption,
                ResponsiblePersonId: $scope.MaterialInput.MaterialInputResponsiblePersonId,
                ResponsiblePerson: $scope.MaterialInput.MaterialInputResponsiblePerson,
                Remarks: $scope.MaterialInput.Remarksss
            });
            $scope.MaterialInput.JobWorkItemId = "";
            $scope.MaterialInput.ItemSpecification = "";
            $scope.MaterialInput.UOM = "";
            $scope.MaterialInput.NetConsumption = "";
            $scope.MaterialInput.Rejection = "";
            $scope.MaterialInput.ValueLoss = "";
            $scope.MaterialInput.GrossConsumption = "";
            $scope.MaterialInput.MaterialInputResponsiblePersonId = "";
            $scope.MaterialInput.MaterialInputResponsiblePerson = "";
            $scope.MaterialInput.Remarksss = "";
        }
    };

    $scope.byProductList = [];
    $scope.AddNewByProductRow = function () {
        $scope.ActionByProduct_Child = 'Add';
        angular.element(document.querySelector("#modalByProduuct")).modal("toggle");
    };
    $scope.AddNewByProduct = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.materialByProductForm.$valid) {
            $scope.byProductList.push({
                Id: '',
                JobWorkItemId: $scope.ByProduct.JobWorkItemId,
                ItemSpecification: $scope.ByProduct.ItemSpecification,
                PercentageOfInput: $scope.ByProduct.PercentageOfInput,
                CurrencyId: $scope.ByProduct.CurrencyId,
                StandardRate: $scope.ByProduct.StandardRate,
                ResponsiblePersonId: $scope.ByProduct.ByProductResponsiblePersonId,
                ResponsiblePerson: $scope.ByProduct.ByProductResponsiblePerson,
                Remarks: $scope.ByProduct.Remarks,
                UOM: $scope.ByProduct.UOM
            });

            $scope.ByProduct.JobWorkItemId = "";
            $scope.ByProduct.ItemSpecification = "";
            $scope.ByProduct.PercentageOfInput = "";
            $scope.ByProduct.CurrencyId = "";
            $scope.ByProduct.StandardRate = "";
            $scope.ByProduct.ByProductResponsiblePersonId = "";
            $scope.ByProduct.ByProductResponsiblePerson = "";
            $scope.ByProduct.Remarks = "";
            $scope.ByProduct.UOM = "";
        }
    };
    $scope.getAllMaterialInput = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetAllMaterialInput?Id=' + $scope.Transformation.Id
        }).then(function successCallback(response) {
            $scope.materialInputList = response.data;
        });
    };

    $scope.getAllByProduct = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetAllByProduct?Id=' + $scope.Transformation.Id
        }).then(function successCallback(response) {
            $scope.byProductList = response.data;
        });
    };


    $scope.materialNameList = [];
    $scope.getJobWorkMaterialNames = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetJobWorkItemNames'
        }).then(function successCallback(response) {
            $scope.materialNameList = response.data;
        });
    };

    $scope.ChangeMaterial = function (JobWorkItemId, index) {
        $http({
            method: 'GET',
            url: $scope.path + 'GetMaterialMasterUOM?Id=' + JobWorkItemId
        }).then(function successCallback(response) {
            $scope.materialInputList[index].UOM = response.data[0]["UOMId"];
            $scope.materialInputList[index].JobWorkItemId = MaterialMasterId;
        });
    };

    $scope.ChangeByProductMaterial = function (JobWorkItemId, index) {
        $http({
            method: 'GET',
            url: $scope.path + 'GetMaterialMasterUOM?Id=' + JobWorkItemId
        }).then(function successCallback(response) {
            $scope.byProductList[index].UOM = response.data[0]["UOMId"];
            $scope.byProductList[index].JobWorkItemId = MaterialMasterId;
        });
    };

    $scope.GetChangeUOMList = [];
    $scope.ChangePopupMaterialUOM = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetMaterialMasterUOM?Id=' + $scope.MaterialInput.JobWorkItemId
        }).then(function successCallback(response) {
            $scope.GetChangeUOMList = response.data;
            if ($scope.GetChangeUOMList.length > 0) {
                if ($scope.GetChangeUOMList[0].MaterialId != null) {
                    $scope.MaterialInput.Material = response.data[0]["Material"];
                    $scope.MaterialInput.UOM = response.data[0]["UOMId"];
                }
                else {
                    $scope.MaterialInput.Material = null;
                    $scope.MaterialInput.UOM = response.data[0]["UOMId"];
                }     
            }          
        });
    };

    $scope.GetChangeByProductUOMList = [];
    $scope.ChangePopupByProductUOM = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetMaterialMasterUOM?Id=' + $scope.ByProduct.JobWorkItemId
        }).then(function successCallback(response) {
            $scope.GetChangeByProductUOMList = response.data;
            if ($scope.GetChangeByProductUOMList.length > 0) {
                if ($scope.GetChangeByProductUOMList[0].MaterialId != null) {
                    $scope.ByProduct.Material = response.data[0]["Material"];
                    $scope.ByProduct.UOM = response.data[0]["UOMId"];
                }
                else {
                    $scope.ByProduct.Material = null;
                    $scope.ByProduct.UOM = response.data[0]["UOMId"];
                }
            }
        });
    };

    $scope.CalculateMaterialInputGrossConsumption = function () {
        var NetConsumption = isNaN($scope.MaterialInput.NetConsumption) ? 0 : $scope.MaterialInput.NetConsumption;
        var ValueLoss = isNaN($scope.MaterialInput.ValueLoss) ? 0 : $scope.MaterialInput.ValueLoss;

        if (NetConsumption !== null && ValueLoss !== null) {
            $scope.MaterialInput.GrossConsumption = (NetConsumption / ((100 - ValueLoss) / 100)).toFixed(2);
        }
        else {
            $scope.MaterialInput.GrossConsumption = 0;
        }
    }

    $scope.grdCalculateMaterialInputGrossConsumption = function (NetConsumption, ValueLoss, index) {
        var NetConsumption = isNaN(NetConsumption) ? 0 : NetConsumption;
        var ValueLoss = isNaN(ValueLoss) ? 0 : ValueLoss;

        if (NetConsumption !== null && ValueLoss !== null) {
            $scope.materialInputList[index].GrossConsumption = (NetConsumption / ((100 - ValueLoss) / 100)).toFixed(2);
        }
        else {
            $scope.materialInputList[index].GrossConsumption = 0;
        }
    };


    $scope.CalculateByProductGrossConsumption = function () {
        var StandardQty = isNaN($scope.ByProduct.StandardQty) ? 0 : $scope.ByProduct.StandardQty;
        var Rejection = isNaN($scope.ByProduct.Rejection) ? 0 : $scope.ByProduct.Rejection;
        var ValueLoss = isNaN($scope.ByProduct.ValueLoss) ? 0 : $scope.ByProduct.ValueLoss;

        if (StandardQty !== null && Rejection !== null && ValueLoss !== null) {
            $scope.ByProduct.GrossQty = (StandardQty / ((100 - (Rejection - ValueLoss)) / 100)).toFixed(2);
        }
        else {
            $scope.ByProduct.GrossQty = 0;
        }
    }

    $scope.grdCalculateByProductGrossConsumption = function (StandardQty, Rejection, ValueLoss, index) {
        var StandardQty = isNaN(StandardQty) ? 0 : StandardQty;
        var Rejection = isNaN(Rejection) ? 0 : Rejection;
        var ValueLoss = isNaN(ValueLoss) ? 0 : ValueLoss;

        if (StandardQty !== null && Rejection !== null && ValueLoss !== null) {
            $scope.byProductList[index].GrossQty = (StandardQty / ((100 - (Rejection - ValueLoss)) / 100)).toFixed(2);
        }
        else {
            $scope.byProductList[index].GrossQty = 0;
        }
    };

    $scope.Cancel = function () {
        $scope.Transformation = {};
        $scope.MaterialInput = {};
        $scope.ByProduct = {};
        $scope.Transformation.Id = '';
        $scope.Transformation.MinRate = '';
        $scope.Transformation.MaxRate = '';
        $scope.Transformation.CycleTime = '';
        $scope.Transformation.ResponsiblePersonId = '';
        $scope.Transformation.ResponsiblePerson = '';
        $scope.Transformation.Remarks = '';
        $scope.getAllProcessName();
        $scope.tab = 1;
        $scope.byProductList = [];
        $scope.materialInputList = [];
        $scope.ShowActivity = true;
        $scope.LabelActivity = false;

        $rootScope.toggle();

    };

    $scope.Clear = function () {
        $scope.Transformation = {};
        $scope.MaterialInput = {};
        $scope.ByProduct = {};
        $scope.Transformation.Id = '';
        $scope.Transformation.MinRate = '';
        $scope.Transformation.MaxRate = '';
        $scope.Transformation.CycleTime = '';
        $scope.Transformation.ResponsiblePersonId = '';
        $scope.Transformation.ResponsiblePerson = '';
        $scope.Transformation.Remarks = '';
        $scope.getAllProcessName();
        $scope.tab = 1;
        $scope.byProductList = [];
        $scope.materialInputList = [];
        $scope.ShowActivity = true;
        $scope.LabelActivity = false;

    };

    $scope.getAllProcessName();
    $scope.getAllActivityUserName();
    $scope.getAllMaterialInput();
    $scope.getAllData();
    $scope.getJobWorkMaterialNames();

    // Validate Max Rate

    $scope.ValidateMaxRate = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.Transformation.MinRate) && !baseService.isUndefinedOrNull($scope.Transformation.MaxRate)) {
                var MRate = parseFloat($scope.Transformation.MinRate);
                var MxRate = parseFloat($scope.Transformation.MaxRate);
                if (MRate > MxRate) {
                //    $scope.Transformation.MaxRate = parseFloat(0);
                    throw "Minimum Rate should be less tha Maximum Rate";
                }
            }
        }
        catch (e) {
            ShowResult(e, "failure");
            throw e;
        }
    }

    // Add Service

    $scope.serviceCboList = [];
    $http.get('Setups/CompanyServiceMaster/GetCboList')
        .then(function (response) {
            $scope.serviceCboList = response.data;
        });
};