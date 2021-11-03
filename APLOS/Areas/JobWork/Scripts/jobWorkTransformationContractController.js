'use strict';
JobWorkTransformationContractController.$inject = ['addressService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function JobWorkTransformationContractController(addressService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Transformation Contract";
    $scope.Action = 'Save';
    $scope.Requirement_Save = "Save";
    $scope.path = 'JobWork/JobWorkTransformationContract/';
    $scope.partyType = "Vendor";
    $scope.tab = 1;

    $scope.ValueAddedContract = {
        Id: '',
        ProcessTypeId: null,
        ContractDate: null,
        ContractTime: null,
        PlantId: null,
        EntityId: null,
        JobWorkLocationId: null,
        MaterialType: null,
        FinalOutputCategory: null,
        PartyId: null,
        PartyCode: null,
        PartyName: null,
        ProcessStartDate: null,
        ProcessEndDate: null,
        ContractClosingDate: null,
        Remarks: ''
    };

    $scope.MaterialPlanning = {
        Id: '',
        JobWorkMaterialMasterId: null,
        JobWorkMaterialName: null,
        MaterialSpecification: null,
        MaterialRef: '',
        UOM: '',
        Quantity: '',
        MaterialMasterName: '',
        MaterialMasterId: '',
        OrderSpecific: null,
        RequiredCapacityPerDay: null,
        ByProductApplicable: null,
        RateApply: null,
        CurrencyId: null,
        Currency: '',
        RatePerUnit: null,
        Rejection: null,
        ValueLoss: null,
        ResponsiblePersonId: null,
        ResponsiblePerson: null,
        Remarks: ''
    }

    $scope.MaterialInput = {
        Id: '',
        JobWorkTransformationContractId: null,
        JobWorkMaterialId: null,
        JobWorkMaterialName: null,
        MaterialName: '',
        MaterialSpecification: null,
        UOM: null,
        NetConsumption: null,
        Rejection: null,
        ValueLoss: null,
        GrossConsumption: null,
        ResponsiblePerson: '',
        ResponsiblePersonId: null,
        Remarks: ''
    };

    $scope.ByProduct = {
        Id: '',
        JobWorkTransformationContractMaterialPlanningId: null,
        JobWorkMaterialId: '',
        MaterialName: '',
        MaterialSpecification: null,
        UOM: null,
        StandardQty: null,
        Rejection: null,
        ValueLoss: null,
        GrossQty: null,
        CurrencyId: null,
        Currency: '',
        StandardRate: null,
        ResponsiblePerson: '',
        ResponsiblePersonId: null,
        Remarks: ''
    };

    $scope.MaterialPlanningAttachment = {
        Id: ''
    }

    $scope.Requirement = {
        Id: '',
        JobWorkTransformationContractMaterialPlanningId: null,
        OrderType: null,
        CustomerId: null,
        ProductionOrderId: null,
        ProductionOrderName: null,
        Specification: null,
        OutputMaterialUOM: '',
        Quantity: null,
        Remarks: ''
    }

    $scope.materialPlanningList = [];
    $scope.AddNewMaterialPlanningRow = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.frmMaterialPlanning.$valid) {
            $scope.materialPlanningList.push({
                Id: '',
                JobWorkMaterialMasterId: $scope.MaterialPlanning.JobWorkMaterialMasterId,
                JobWorkMaterialName: $("#ddlJobWorkMaterialMasterId option:selected").text(),
                MaterialSpecification: $scope.MaterialPlanning.MaterialSpecification,
                MaterialRef: $scope.MaterialPlanning.MaterialRef,
                UOM: $scope.MaterialPlanning.UOM,
                Quantity: $scope.MaterialPlanning.Quantity,
                MaterialMasterId: $scope.MaterialPlanning.MaterialMasterId,
                MaterialMasterName: $scope.MaterialPlanning.MaterialMasterName,
                OrderSpecific: $scope.MaterialPlanning.OrderSpecific,
                RequiredCapacityPerDay: $scope.MaterialPlanning.RequiredCapacityPerDay,
                ByProductApplicable: $scope.MaterialPlanning.ByProductApplicable,
                RateApply: $scope.MaterialPlanning.RateApply,
                CurrencyId: $scope.MaterialPlanning.CurrencyId,
                Currency: $("#ddlCurrencyId option:selected").text(),
                RatePerUnit: $scope.MaterialPlanning.RatePerUnit,
                Rejection: $scope.MaterialPlanning.Rejection,
                ValueLoss: $scope.MaterialPlanning.ValueLoss,
                ResponsiblePersonId: $scope.MaterialPlanning.ResponsiblePersonId,
                ResponsiblePerson: $scope.MaterialPlanning.ResponsiblePerson,
                Remarks: $scope.MaterialPlanning.Remarks
            });
            $scope.MaterialPlanning = {};
        }
    }

    $scope.materialInputList = [];
    $scope.AddNewMaterialInputRow = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.frmMaterialInput.$valid) {
            $scope.materialInputList.push({
                Id: '',
                JobWorkTransformationContractId: '',
                JobWorkMaterialId: $scope.MaterialInput.JobWorkMaterialId,
                JobWorkMaterialName: $("#ddlJobWorkMaterialInputMasterId option:selected").text(),
                MaterialSpecification: $scope.MaterialInput.MaterialSpecification,
                UOM: $scope.MaterialInput.UOM,
                NetConsumption: $scope.MaterialInput.NetConsumption,
                Rejection: $scope.MaterialInput.Rejection,
                ValueLoss: $scope.MaterialInput.ValueLoss,
                GrossConsumption: $scope.MaterialInput.GrossConsumption,
                ResponsiblePersonId: $scope.MaterialInput.ResponsiblePersonId,
                ResponsiblePerson: $scope.MaterialInput.ResponsiblePerson,
                Remarks: $scope.MaterialInput.Remarks
            });
            $scope.MaterialInput = {};
        }
    }
    //#region Tab
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    //#endregion
    //#region Partial View
    //$controller("employeeBaseController", { $scope: $scope, $http: $http });
    $controller("employeeBaseMultipleController", { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.materialType = ['Asset', 'Consumable', 'Spare', 'RawMaterial'];

    $scope.closeResponsiblePersonPopUp = function () {
        if ($scope.responsiblePersonIndex !== -1) {
            var employee = $scope.employeeList[$scope.responsiblePersonIndex];
            if ($scope.responsiblePersonHiddenControlId === "Planning") {
                $scope.MaterialPlanning.ResponsiblePerson = employee.EmployeeName;
                $scope.MaterialPlanning.ResponsiblePersonId = employee.SystemId;
            }
            else if ($scope.responsiblePersonHiddenControlId === "Input") {
                $scope.MaterialInput.ResponsiblePerson = employee.EmployeeName;
                $scope.MaterialInput.ResponsiblePersonId = employee.SystemId;
            }
            else if ($scope.responsiblePersonHiddenControlId === "ByProduct") {
                $scope.ByProduct.ResponsiblePerson = employee.EmployeeName;
                $scope.ByProduct.ResponsiblePersonId = employee.SystemId;
            }
        }
        $scope.hideResponsiblePersonPopUp();
    };
    $scope.hideResponsiblePersonPopUp = function () {
        angular.element(document.querySelector("#responsiblePersonPopUp")).modal("hide");
    };

    $scope.selectMaterialByType = function (ob) {
        $scope.MaterialPlanning.MaterialMasterId = ob.Id;
        $scope.MaterialPlanning.MaterialMasterName = ob.Code;
        $scope.closeMaterialMasterbyTypePopUp();
    };

    //#endregion
    //#region Party Popup
    $scope.partyParameters = {
        limit: 10
        , offset: 0
        , order: 'ASC'
        , sort: 'UserName, PartyAccountGroupName'
        , searchBy: 'UserName'
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };

    $scope.showPartyPopUp = function () {
        baseService.setCurrentPage('partyList');
        $scope.getPartyList = function (pageno) {
            if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataList?partyType=' + $scope.partyType;
            }
            else if ($scope.partyType === 'Party') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataList';
            }
            else if ($scope.partyType === 'Director') {
                $scope.partyUrl = 'Parties/party/GetCompanyDirectorDataList';
            }
            else if ($scope.partyType === 'Other') {
                $scope.partyUrl = 'Parties/party/GetCompanyOtherDataList';
            }
            baseService.paginationBase($scope.partyUrl, pageno, $scope.partyParameters)
                .then(function (result) {
                    $scope.partyList = result.Rows;
                    $scope.partyParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#partyPopUp')).modal('show');
        $scope.getPartyList();
    };
    $scope.closePartyPopUp = function (index, Id) {
        $scope.partyIndex = index;
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            $scope.ValueAddedContract.PartyCode = party.Code;
            $scope.ValueAddedContract.PartyName = party.UserName;
            $scope.ValueAddedContract.PartyId = party.Id;
            angular.element(document.querySelector('#partyPopUp')).modal('hide');
        }
    };
    //#endregion
    //#region Dropdown Load
    $scope.materialTypeList = [{ name: 'In-Process' }, { name: 'Inventry' }];
    $scope.finalOutputCategory = [{ name: 'Final Sale' }, { name: 'In-Process' }];
    $scope.orderTypeList = [{ name: 'Internal' }, { name: 'External' }];

    $scope.processTypeList = [];
    $scope.LoadProcessType = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'LoadProcessType'
        }).then(function successCallback(response) {
            $scope.processTypeList = response.data;
        });
    };

    $scope.plantList = [];
    $scope.LoadAllPlant = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'LoadAllPlant'
        }).then(function successCallback(response) {
            $scope.plantList = response.data;
        });
    };

    $scope.entityList = [];
    $scope.LoadPlantWiseEntity = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'LoadPlantWiseEntity?Id=' + $scope.ValueAddedContract.PlantId
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    };

    $scope.materialLocationList = [];
    $scope.LoadMaterialLocation = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'LoadMaterialLocation?PlantId=' + $scope.ValueAddedContract.PlantId + '&&EntityId=' + $scope.ValueAddedContract.EntityId
        }).then(function successCallback(response) {
            $scope.materialLocationList = response.data;
        });
    };
    //#endregion
    //#region Master Data Save
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.frmValueAddedContract.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.path + "SaveData",
                    data: { 'saveData': $scope.ValueAddedContract, 'materialPlanning': $scope.materialPlanningList, 'materialInput': $scope.materialInputList },
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

    //#endregion
    //#region Get, Set Master Grid Data
    $scope.gridDataList = [];
    $scope.getAllData = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetAllData'
        }).then(function successCallback(response) {
            $scope.gridDataList = response.data;
        });
    };

    $scope.recorddoubleclick = function ($event) {
        var x = $event;
        $scope.RowId = x.data.Id;
        $scope.PopulateSelectedData($scope.RowId);
        $scope.LoadMaterialPlanningData($scope.RowId);
        $scope.LoadMaterialInputData($scope.RowId);
    };

    $scope.LoadSelectedData = function (Id) {
        var x = "#" + Id;
        var gridObj = $(x).data("ejGrid");
        $scope.selecteddata = gridObj.getSelectedRecords()[0];
        $scope.ValueAddedContract.Id = $scope.selecteddata.Id;
        $scope.PopulateSelectedData($scope.ValueAddedContract.Id);
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
                $scope.ValueAddedContract.Id = response.data[0].Id;
                $scope.ValueAddedContract.ProcessTypeId = response.data[0].ProcessTypeId;
                $("#txtContractDate").ejDatePicker({ value: new Date(response.data[0].ContractDate) });
                $("#txtContractTime").ejTimePicker({ value: response.data[0].ContractTime });
                $scope.ValueAddedContract.PlantId = response.data[0].PlantId;
                $scope.LoadPlantWiseEntity();
                $scope.ValueAddedContract.EntityId = response.data[0].EntityId;
                $scope.LoadMaterialLocation();
                $scope.ValueAddedContract.JobWorkLocationId = response.data[0].JobWorkLocationId;
                $scope.ValueAddedContract.MaterialType = response.data[0].MaterialType;
                $scope.ValueAddedContract.FinalOutputCategory = response.data[0].FinalOutputCategory;
                $scope.ValueAddedContract.PartyId = response.data[0].PartyId;
                $scope.ValueAddedContract.PartyCode = response.data[0].PartyCode;
                $scope.ValueAddedContract.PartyName = response.data[0].PartyName;
                $("#txtProcessStartDate").ejDatePicker({ value: new Date(response.data[0].ProcessStartDate) });
                $("#txtProcessEndDate").ejDatePicker({ value: new Date(response.data[0].ProcessEndDate) });
                $("#txtContractClosingDate").ejDatePicker({ value: new Date(response.data[0].ContractClosingDate) });
                $scope.ValueAddedContract.Remarks = response.data[0].Remarks;

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

    $scope.LoadMaterialPlanningData = function (Id) {
        $http({
            method: 'POST',
            url: $scope.path + 'GetMaterialPlanningData',
            data: {
                'Id': Id
            }
        }).then(function successCallback(response) {
            $scope.materialPlanningList = response.data;
        });
    }

    $scope.LoadMaterialInputData = function (Id) {
        $http({
            method: 'POST',
            url: $scope.path + 'GetMaterialInputData',
            data: {
                'Id': Id
            }
        }).then(function successCallback(response) {
            $scope.materialInputList = response.data;
        });
    }
    //#endregion
    //#region Material planning
    $scope.AddNewMaterialPlanning = function () {
        $scope.MaterialPlanning_Child = 'Add';
        angular.element(document.querySelector("#modalMaterialPlanning")).modal("toggle");
    };

    $scope.orderSpecificList = [{ name: 'Yes' }, { name: 'No' }];

    $scope.valueAddedMasterItemList = [];
    $scope.getValueAddedMasterItem = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetValueAddedMasterItem'
        }).then(function successCallback(response) {
            $scope.valueAddedMasterItemList = response.data;
        });
    };

    $scope.LoadItemDetails = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetSelectedValueLossRejectionUOM?Id=' + $scope.MaterialPlanning.JobWorkMaterialMasterId
        }).then(function successCallback(response) {
            $scope.MaterialPlanning.UOM = response.data[0].UOMName;
            $scope.MaterialPlanning.ValueLoss = response.data[0].StdValueLoss;
            $scope.MaterialPlanning.Rejection = response.data[0].StdRejection;
            $scope.MaterialPlanning.RateApply = response.data[0].RateApplicable;
            $scope.MaterialPlanning.CurrencyId = response.data[0].CurrencyId;
        });
    };

    $scope.LoadItemUOM = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetJobWorkMaterialUOM?Id=' + $scope.MaterialInput.JobWorkMaterialId
        }).then(function successCallback(response) {
            $scope.MaterialInput.UOM = response.data[0].UserName;
        });
    };

    $scope.rateApplicableList = [];
    $scope.getMaterialPlanningRate = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetRateApplicable'
        }).then(function successCallback(response) {
            $scope.rateApplicableList = response.data;
        });
    };

    $scope.materialPlanningCurrencyList = [];
    $scope.getMaterialPlanningCurrency = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetValueAddedCurrency'
        }).then(function successCallback(response) {
            $scope.materialPlanningCurrencyList = response.data;
        });
    };

    $scope.buyerList = [];
    $scope.getBuyerList = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetBuyerList'
        }).then(function successCallback(response) {
            $scope.buyerList = response.data;
        });
    };

    $scope.byProductApplicableList = [];
    $scope.getByProductApplicable = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetByProductApplicable'
        }).then(function successCallback(response) {
            $scope.byProductApplicableList = response.data;
        });
    };

    $scope.productionOrderList = [];
    $scope.LoadProductionOrder = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetProductionOrder'
        }).then(function successCallback(response) {
            $scope.productionOrderList = response.data;
            angular.element(document.querySelector("#modalProductionOrder")).modal("toggle");
        });
    };

    $scope.SetRequirements = function (index, data) {
        try {
            $scope.Index = index;
            $scope.Requirement.JobWorkTransformationContractMaterialPlanningId = data.Id;
            $scope.LoadAllRequirements($scope.Requirement.JobWorkTransformationContractMaterialPlanningId);
            angular.element(document.querySelector('#modalMaterialPlanningRequirement')).modal('toggle');
        } catch (e) {
            ShowResult(e, "failure");
        }
    }

    $scope.recorddoubleProductionclick = function ($event) {
        var x = $event;
        $scope.RowId = x.data.Id;
        $scope.Requirement.ProductionOrderId = x.data.Id;
        $scope.Requirement.ProductionOrderName = x.data.MasterOrderId;
        angular.element(document.querySelector('#modalProductionOrder')).modal('toggle');
    };

    $scope.removeMaterialPlanning = function (index, data) {
        try {
            $scope.Index = index;
            $scope.Id = data.Id;
            $scope.message = 'Are you sure want to delete this data....';
            angular.element(document.querySelector('#removeMaterialPlanningPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.removeMaterialPlanningRow = function () {
        if (!baseService.isUndefinedOrNull($scope.Index) && $scope.Id == "") {
            $scope.materialPlanningList.splice($scope.Index, 1);
        }
        else {
            $http({
                method: 'POST',
                url: $scope.path + 'DeleteMaterialPlanningChildData?Id=' + $scope.Id
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.materialPlanningList.splice($scope.Index, 1);
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };

    //#endregion
    $scope.AddNewMaterialInput = function () {
        $scope.MaterialInput_Child = 'Add';
        angular.element(document.querySelector("#modalMaterialInput")).modal("toggle");
    };

    $scope.jobWorkMaterialList = [];
    $scope.getJobWorkMaterial = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetJobWorkMaterialList'
        }).then(function successCallback(response) {
            $scope.jobWorkMaterialList = response.data;
        });
    };
    $scope.removeMaterialInput = function (index, data) {
        try {
            $scope.Index = index;
            $scope.Id = data.Id;
            $scope.message_confirmationMaterialInput = 'Are you sure want to delete this data....';
            angular.element(document.querySelector('#confirmMaterialInputDeletePopUp')).modal('show');
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.removeMaterialInputRow = function () {
        if (!baseService.isUndefinedOrNull($scope.Index) && $scope.Id == "") {
            $scope.materialInputList.splice($scope.Index, 1);
        }
        else {
            $http({
                method: 'POST',
                url: $scope.path + 'DeleteMaterialInputChildData?Id=' + $scope.Id
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
    $scope.EditMaterialInput = function (index, data) {
        try {
            $scope.Index = index;
            $scope.Id = data.Id;

            $http({
                method: 'POST',
                url: $scope.path + 'GetSelectedMaterialInputData?Id=' + $scope.Id
            }).then(function successCallback(response) {
                $scope.MaterialInput.Id = response.data[0].Id;
                $scope.MaterialInput.JobWorkTransformationContractId = response.data[0].JobWorkTransformationContractId;
                $scope.MaterialInput.JobWorkMaterialId = response.data[0].JobWorkMaterialId;
                $scope.MaterialInput.JobWorkMaterialName = response.data[0].JobWorkMaterialName;
                $scope.MaterialInput.MaterialSpecification = response.data[0].MaterialSpecification;
                $scope.MaterialInput.UOM = response.data[0].UOM;
                $scope.MaterialInput.NetConsumption = response.data[0].NetConsumption;
                $scope.MaterialInput.Rejection = response.data[0].Rejection;
                $scope.MaterialInput.ValueLoss = response.data[0].ValueLoss;
                $scope.MaterialInput.GrossConsumption = response.data[0].GrossConsumption;
                $scope.MaterialInput.ResponsiblePersonId = response.data[0].ResponsiblePersonId;
                $scope.MaterialInput.ResponsiblePerson = response.data[0].ResponsiblePerson;
                $scope.MaterialInput.Remarks = response.data[0].Remarks;
                $scope.MaterialInput_Child = "Update";
            });

            angular.element(document.querySelector('#modalMaterialInput')).modal('toggle');
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    //BY Product
    $scope.jobWorkByProductMaterialList = [];
    $scope.getJobWorkMaterialByProduct = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetJobWorkMaterialListByProduct'
        }).then(function successCallback(response) {
            $scope.jobWorkByProductMaterialList = response.data;
        });
    };
    $scope.SetByProductApplicable = function (index, data) {
        try {
            $scope.Index = index;
            $scope.ByProduct.JobWorkTransformationContractMaterialPlanningId = data.Id;
            $scope.ByProduct_Save = "Save";
            $scope.LoadAllByProductList($scope.ByProduct.JobWorkTransformationContractMaterialPlanningId);
            angular.element(document.querySelector('#modalByProductApplicable')).modal('toggle');
        } catch (e) {
            ShowResult(e, "failure");
        }
    }

    $scope.ChangeMaterialPlanningByProductUOM = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetJobWorkMaterialUOM?Id=' + $scope.ByProduct.JobWorkMaterialId
        }).then(function successCallback(response) {
            $scope.ByProduct.UOM = response.data[0].UserName;
        });
    }

    $scope.SaveMaterialPlanningByProduct = function () {
        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.frmByProduct.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.path + "SaveMaterialPlanningByProduct",
                    data: { 'saveData': $scope.ByProduct },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.LoadAllByProductList($scope.ByProduct.JobWorkTransformationContractMaterialPlanningId);
                        $scope.ByProduct_Save = "Save";

                        $scope.ByProduct.Id = "";
                        $scope.ByProduct.JobWorkMaterialId = "";
                        $scope.ByProduct.MaterialName = "";
                        $scope.ByProduct.MaterialSpecification = "";
                        $scope.ByProduct.UOM = "";
                        $scope.ByProduct.StandardQty = "";
                        $scope.ByProduct.Rejection = "";
                        $scope.ByProduct.ValueLoss = "";
                        $scope.ByProduct.GrossQty = "";
                        $scope.ByProduct.CurrencyId = "";
                        $scope.ByProduct.Currency = "";
                        $scope.ByProduct.StandardRate = "";
                        $scope.ByProduct.ResponsiblePerson = "";
                        $scope.ByProduct.ResponsiblePersonId = "";
                        $scope.ByProduct.Remarks = "";

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

    $scope.gridByProductList = [];
    $scope.LoadAllByProductList = function (Id) {
        $http({
            method: 'POST',
            url: $scope.path + 'GetByProductList?Id=' + Id
        }).then(function successCallback(response) {
            $scope.gridByProductList = response.data;
        });
    }

    $scope.recordByProductdoubleclick = function ($event) {
        var x = $event;
        $scope.RowId = x.data.Id;
        $scope.ByProduct.Id = x.data.Id;
        $scope.LoadSelectedByProduct($scope.ByProduct.Id);
    };
    $scope.LoadSelectedByProduct = function (Id) {
        $http({
            method: 'POST',
            url: $scope.path + 'GetSelectedByProduct?Id=' + Id
        }).then(function successCallback(response) {
            $scope.ByProduct.Id = response.data[0].Id;
            $scope.ByProduct.JobWorkTransformationContractMaterialPlanningId = response.data[0].JobWorkTransformationContractMaterialPlanningId;
            $scope.ByProduct.JobWorkMaterialId = response.data[0].JobWorkMaterialId;
            $scope.ByProduct.MaterialName = response.data[0].MaterialName;
            $scope.ByProduct.MaterialSpecification = response.data[0].MaterialSpecification;
            $scope.ByProduct.UOM = response.data[0].UOM;
            $scope.ByProduct.StandardQty = response.data[0].StandardQty;
            $scope.ByProduct.Rejection = response.data[0].Rejection;
            $scope.ByProduct.ValueLoss = response.data[0].ValueLoss;
            $scope.ByProduct.GrossQty = response.data[0].GrossQty;
            $scope.ByProduct.CurrencyId = response.data[0].CurrencyId;
            $scope.ByProduct.Currency = response.data[0].Currency;
            $scope.ByProduct.StandardRate = response.data[0].StandardRate;
            $scope.ByProduct.ResponsiblePerson = response.data[0].ResponsiblePerson;
            $scope.ByProduct.ResponsiblePersonId = response.data[0].ResponsiblePersonId;
            $scope.ByProduct.Remarks = response.data[0].Remarks;

            $scope.ByProduct_Save = "Update";
        });
    }
    $scope.removeMaterialPlanningByProduct = function (Id) {
        try {
            var x = "#" + Id;
            var gridObj = $(x).data("ejGrid");
            $scope.selecteddata = gridObj.getSelectedRecords()[0];
            $scope.ByProduct.Id = $scope.selecteddata.Id;

            $scope.message_confirmationByProduct = 'Are you sure want to Remove?';
            angular.element(document.querySelector('#confirmByProductDeletePopUp')).modal('show');
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.removeByProductRow = function () {
        try {
            $http({
                method: 'GET',
                url: $scope.path + "DeleteSelectedByProductRow?Id=" + $scope.ByProduct.Id
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    $scope.LoadAllByProductList($scope.ByProduct.JobWorkTransformationContractMaterialPlanningId);
                    ShowResult(response.data.Message, 'success');
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    //#region Attachment
    $("#filec").change(function () {
        $scope.filedata = this.files[0];
    });
    $scope.SetAttachment = function (index, data) {
        try {
            $scope.Index = index;
            $scope.MaterialPlanning.Id = data.Id;
            $scope.LoadAttachmentList($scope.MaterialPlanning.Id);
            angular.element(document.querySelector('#modalMaterialPlanningAttachment')).modal('show');
        } catch (e) {
            ShowResult(e, "failure");
        }
    }
    $scope.SaveMaterialPlanningAttachmentFile = function () {
        var data = new FormData();
        data.append("file", $scope.filedata);
        data.append("Id", $scope.MaterialPlanning.Id.toString());

        try {
            $http({
                method: "POST",
                url: $scope.path + "SaveMaterialPlanningAttachment",
                withCredentials: true,
                processData: false,
                headers: { 'Content-Type': undefined },
                contentType: undefined,
                dataType: JSON,
                data: data,
                transformRequest: angular.identity
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    $scope.LoadAttachmentList($scope.MaterialPlanning.Id);
                    ShowResult(response.data.Message, "success");
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });

        } catch (e) {
            ShowResult(e, "failure");
        }
    }
    $scope.removeMaterialPlanningAttachment = function (Id) {
        try {
            var x = "#" + Id;
            var gridObj = $(x).data("ejGrid");
            $scope.selecteddata = gridObj.getSelectedRecords()[0];
            $scope.MaterialPlanningAttachment.Id = $scope.selecteddata.Id;

            $scope.message_confirmationAttachment = 'Are you sure want to Remove?';
            angular.element(document.querySelector('#confirmAttachmentDeletePopUp')).modal('show');
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.removeAttachmentRow = function () {
        try {
            $http({
                method: 'GET',
                url: $scope.path + "DeleteMaterialPlanningAttachment?Id=" + $scope.MaterialPlanningAttachment.Id
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');

                    $scope.LoadAttachmentList($scope.MaterialPlanning.Id);
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.gridAttachmentList = [];
    $scope.LoadAttachmentList = function (Id) {
        $http({
            method: 'POST',
            url: $scope.path + 'GetMaterialPlanningAttachment?Id=' + Id
        }).then(function successCallback(response) {
            $scope.gridAttachmentList = response.data;
        });
    }
    //#endregion
    //#region Requirements
    $scope.SaveMaterialPlanningRequirement = function () {
        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.frmValueAddedRequirement.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.path + "SaveMaterialPlanningRequirement",
                    data: { 'saveData': $scope.Requirement },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.LoadAllRequirements($scope.Requirement.JobWorkTransformationContractMaterialPlanningId);
                        $scope.Requirement.Id = "";
                        $scope.Requirement.OrderType = "";
                        $scope.Requirement.CustomerId = "";
                        $scope.Requirement.ProductionOrderId = "";
                        $scope.Requirement.ProductionOrderName = "";
                        $scope.Requirement.Specification = "";
                        $scope.Requirement.OutputMaterialUOM = "";
                        $scope.Requirement.Quantity = "";
                        $scope.Requirement.Remarks = "";
                        $scope.Requirement_Save = "Save";
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
    $scope.materialPlanningRequirementsList = [];
    $scope.LoadAllRequirements = function (Id) {
        $http({
            method: 'POST',
            url: $scope.path + 'GetAllMaterialPlanningRequirements',
            data: {
                'Id': Id
            }
        }).then(function successCallback(response) {
            $scope.materialPlanningRequirementsList = response.data;
        });
    }
    $scope.recordRequirementdoubleclick = function ($event) {
        var x = $event;
        $scope.RowId = x.data.Id;
        $scope.Requirement.Id = x.data.Id;
        $scope.LoadSelectedRequirementData($scope.Requirement.Id);
    };
    $scope.LoadSelectedRequirementData = function (Id) {
        $http({
            method: 'POST',
            url: $scope.path + 'GetSelectedMaterialPlanningRequirements?Id=' + Id
        }).then(function successCallback(response) {
            $scope.Requirement.Id = response.data[0].Id;
            $scope.Requirement.JobWorkTransformationContractMaterialPlanningId = response.data[0].JobWorkTransformationContractMaterialPlanningId;
            $scope.Requirement.OrderType = response.data[0].OrderType;
            $scope.Requirement.CustomerId = response.data[0].CustomerId;
            $scope.Requirement.ProductionOrderId = response.data[0].ProductionOrderId;
            $scope.Requirement.ProductionOrderName = response.data[0].MasterOrderId;
            $scope.Requirement.Specification = response.data[0].Specification;
            $scope.Requirement.OutputMaterialUOM = response.data[0].OutputMaterialUOM;
            $scope.Requirement.Quantity = response.data[0].Quantity;
            $scope.Requirement.Remarks = response.data[0].Remarks;
            $scope.Requirement_Save = "Update";
        });
    }
    $scope.removeMaterialPlanningRequirement = function (Id) {
        try {
            var x = "#" + Id;
            var gridObj = $(x).data("ejGrid");
            $scope.selecteddata = gridObj.getSelectedRecords()[0];
            $scope.Requirement.Id = $scope.selecteddata.Id;

            $scope.message_confirmationRequirement = 'Are you sure want to Remove?';
            angular.element(document.querySelector('#confirmRequirementDeletePopUp')).modal('show');
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.removeRequirementRow = function () {
        try {
            $http({
                method: 'GET',
                url: $scope.path + "DeleteMaterialPlanningRequirements?Id=" + $scope.Requirement.Id
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadAllRequirements($scope.Requirement.JobWorkTransformationContractMaterialPlanningId);
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    //#endregion

    $scope.Clear = function () {
        $scope.ValueAddedContract = {};
        $scope.ValueAddedContract.Id = "";
        $scope.ValueAddedContract.Remarks = "";
    };

    $scope.Cancel = function () {
        $rootScope.toggle();
        $scope.Clear();
    };

    //#region Initialization
    $scope.getJobWorkMaterialByProduct();
    $scope.getJobWorkMaterial();
    $scope.getByProductApplicable();
    $scope.getBuyerList();
    $scope.getMaterialPlanningCurrency();
    $scope.getMaterialPlanningRate();
    $scope.getValueAddedMasterItem();
    $scope.LoadAllPlant();
    $scope.LoadProcessType();
    $scope.getAllData();
    //#endregion
}