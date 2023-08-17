'use strict';
jobWorkValueAddedMasterController.$inject = ['addressService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function jobWorkValueAddedMasterController(addressService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Jobwork/Outsource Value Added Master";
    $scope.Action = 'Save';
    $scope.path = 'Outsourcing/JobWorkValueAddedMaster/';
    $controller("currencyBaseController", { $scope: $scope, $http: $http });

    $scope.ValueAdded = {
        Id: '',
        JobWorkActivityId: null,
        JobWorkActivityChildId: null,
        MaterialUOMId: null,
        StdRejection: null,
        StdValueLoss: null,
        RateApplicable: null,
        CurrencyId: null,
        MinRate: null,
        MaxRate: null,
        CycleTime: null,
        ResponsiblePerson: null,
        ResponsiblePersonId: null,
        Remarks: '',
        ServiceId: null,
    };

    $scope.rateApplicableList = [{ name: 'Input' }, { name: 'Output' }];
    $controller("employeeBaseController", { $scope: $scope, $http: $http });

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
            $scope.ValueAdded.ResponsiblePerson = employee.EmployeeName;
            $scope.ValueAdded.ResponsiblePersonId = employee.SystemId;
        }
        $scope.hideResponsiblePersonPopUp();
    };
    $scope.hideResponsiblePersonPopUp = function () {
        angular.element(document.querySelector("#responsiblePersonPopUp")).modal("hide");
    };

    $scope.Save = function () {
        $scope.ValidateMaxRate();
        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.jobWorkValueAddedMaster.$valid) {

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

                $http({
                    method: 'POST',
                    url: $scope.path + "SaveData",
                    data: { 'saveData': $scope.ValueAdded, 'childData': $scope.selProcessList },
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
    $scope.recorddoubleclick = function ($event) {
        var x = $event;
        $scope.RowId = x.data.Id;
        $scope.PopulateSelectedData($scope.RowId);
    };

    $scope.LoadSelectedData = function (Id) {
        var x = "#" + Id;
        var gridObj = $(x).data("ejGrid");
        $scope.selecteddata = gridObj.getSelectedRecords()[0];
        $scope.ValueAdded.Id = $scope.selecteddata.Id;
        $scope.PopulateSelectedData($scope.ValueAdded.Id);
    };

    $scope.DisableJWActivity = false;

    $scope.PopulateSelectedData = function (Id) {
        $http({
            method: 'POST',
            url: $scope.path + 'GetSelectedData',
            data: {
                'Id': Id
            }
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.ValueAdded.Id = response.data[0].Id;
                $scope.ValueAdded.JobWorkActivityId = response.data[0].JobWorkActivityId;
                $scope.getActivityChildItems();
                $scope.ValueAdded.JobWorkActivityChildId = response.data[0].JobWorkActivityChildId;
                $scope.getJobWorkItemUOM();
                $scope.ValueAdded.StdRejection = response.data[0].StdRejection;
                $scope.ValueAdded.StdValueLoss = response.data[0].StdValueLoss;
                $scope.ValueAdded.RateApplicable = response.data[0].RateApplicable;
                $scope.ValueAdded.CurrencyId = response.data[0].CurrencyId;
                $scope.ValueAdded.MinRate = response.data[0].MinRate;
                $scope.ValueAdded.MaxRate = response.data[0].MaxRate;
                $scope.ValueAdded.CycleTime = response.data[0].CycleTime;
                $scope.ValueAdded.ResponsiblePerson = response.data[0].ResponsiblePerson;
                $scope.ValueAdded.ResponsiblePersonId = response.data[0].ResponsiblePersonId;
                $scope.ValueAdded.Remarks = response.data[0].Remarks;
                $scope.ValueAdded.ServiceId = response.data[0].ServiceId;

                $scope.getSelectedProcessData();

                $scope.Action = 'Update';
                $scope.DisableJWActivity = true;

                if (!$rootScope.isCollapsed) {
                    $rootScope.toggle();
                }
            }
            else {
                ShowResult('No Data Found..!', 'failure');
            }
        });
    };

    $scope.getJobWorkItemUOM = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getJobWorkItemUOM?Id=' + $scope.ValueAdded.JobWorkActivityChildId
        }).then(function successCallback(response) {
            $scope.materialUOMList = response.data;
            if ($scope.materialUOMList.length > 0) {
                if ($scope.materialUOMList[0].MaterialMasterId != null) {
                    $scope.ValueAdded.MaterialCode = $scope.materialUOMList[0].MaterialCode;
                    $scope.ValueAdded.MaterialName = $scope.materialUOMList[0].Material;
                    $scope.ValueAdded.MaterialUOMId = $scope.materialUOMList[0].MMUnit;
                }
                else {
                    
                    $scope.ValueAdded.MaterialCode = null;
                    $scope.ValueAdded.MaterialName = null;
                    $scope.ValueAdded.MaterialUOMId = null;
                    $scope.ValueAdded.MaterialUOMId = $scope.materialUOMList[0].JWIUnit;
                }
            }

        });
    };

    $scope.DeleteSelectedData = function () {
        $scope.message_confirmation = 'Are you sure want to Delete?';
        angular.element(document.querySelector('#confirmDeletePopUp')).modal('show');
    };

    $scope.removeRow = function () {
        try {
            $http({
                method: 'GET',
                url: $scope.path + "DeleteSelectedData?Id=" + $scope.ValueAdded.Id
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

    $scope.currencyList = [];
    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = [];
        $scope.currencyList = result;
    });

    $scope.gridDataList = [];
    $scope.getAllData = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetAllData'
        }).then(function successCallback(response) {
            $scope.gridDataList = response.data;
            $scope.DisableJWActivity = false;
        });
    };

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
        $scope.processList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'GetAllProcessName'
        }).then(function successCallback(response) {
            $scope.processList = response.data;
        });
    };

    //$scope.materialUOMList = [];
    //$scope.getJobWorkItemUOM = function () {
    //    $http({
    //        method: 'GET',
    //        url: $scope.path + 'GetJobWorkMaterialUOM?Id=' + $scope.ValueAdded.JobWorkActivityChildId
    //    }).then(function successCallback(response) {
    //        $scope.ValueAdded.MaterialUOMId = response.data[0].UserName;
    //    });
    //};

    $scope.activityChildItemsList = [];
    $scope.getActivityChildItems = function () {
        $scope.ValueAdded.MaterialCode = null;
        $scope.ValueAdded.MaterialName = null;
        $scope.ValueAdded.MaterialUOMId = null;
        $http({
            method: 'GET',
            url: $scope.path + 'GetActivityChildItems?Id=' + $scope.ValueAdded.JobWorkActivityId
        }).then(function successCallback(response) {
            $scope.activityChildItemsList = response.data;
        });
    };

    $scope.getSelectedProcessData = function () {
   //     $scope.processList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'GetSelectedProcessData?Id=' + $scope.ValueAdded.Id
        }).then(function successCallback(response) {
             
            var DropDownListObj = $("#ddlProcess").data("ejDropDownList");
            DropDownListObj.uncheckAll();
            for (var j = 0; j < response.data.length; j++) {

                DropDownListObj.selectItemByValue(response.data[j].ProcessId);
        //        $scope.processList.selectItemByValue(response.data[j].ProcessId);
            }
        });
    };

    $scope.getProcessID = function (Id, index) {
        $scope.gridChildDataList[index].ProcessId = Id;
    };

    $scope.removeRowModal = function (index, data) {
        try {
            $scope.Index = index;
            $scope.Id = data.Id;
            $scope.message = 'Are you sure want to delete this data....';
            angular.element(document.querySelector('#removerPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.removeChildRow = function () {
        if (!baseService.isUndefinedOrNull($scope.Index) && $scope.Id === "") {
            $scope.gridChildDataList.splice($scope.Index, 1);
        }
        else {
            $http({
                method: 'POST',
                url: $scope.path + 'DeleteChildData?Id=' + $scope.Id
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.gridChildDataList.splice($scope.Index, 1);
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };
    $scope.Clear = function () {
        $scope.ValueAdded = {};
        $scope.ValueAdded.Id = '';
        $scope.ValueAdded.ResponsiblePerson = '';
        $scope.ValueAdded.ResponsiblePersonId = '';
        $scope.ValueAdded.MinRate = '';
        $scope.ValueAdded.MaxRate = '';
        $scope.ValueAdded.CycleTime = '';
        $scope.ValueAdded.Remarks = '';
        $scope.getAllProcessName();
        $scope.DisableJWActivity = false;
        $scope.processList = [];
    };
    $scope.Cancel = function () {
        $scope.Clear();
        $rootScope.toggle();
    };

    //$scope.getJobWorkMaterialUOM();
    $scope.getAllProcessName();
    $scope.getAllActivityUserName();
    $scope.getAllData();
    $scope.Clear();

    // Validate Max Rate

    $scope.ValidateMaxRate = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.ValueAdded.MinRate) && !baseService.isUndefinedOrNull($scope.ValueAdded.MaxRate) ) {
                var MRate = parseFloat($scope.ValueAdded.MinRate);
                var MxRate = parseFloat($scope.ValueAdded.MaxRate);
                if (MRate > MxRate) {
                //    $scope.ValueAdded.MaxRate = parseFloat(0);
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