'use strict';
ProductionTransformationBookingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ProductionTransformationBookingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Production Booking/ Transformation';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Productions/ProductionTransformationBooking/';
    $scope.getListUrl = $scope.path + 'getlist';
 //   $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "ProcessSetStandardName"; $scope.search = "";
    $scope.searchByList = [{ value: 'ProcessSetStandardName', name: "Process Set StandardName" }, { value: 'ProcessSetUserName', name: "Process Set UserName" }, { value: 'ProcessSetShortName', name: "Process Set ShortName" }, { value: 'ProcessSetUserCode', name: "Process Set UserCode" }];

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {          
            $scope.ModelList = response.data;
            ClearFields();
    //        $scope.GetSequence();
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        ProcessSetStandardName: null,
        ProcessSetUserName: null,
        ProcessSetShortName: null,
        ProcessSetUserCode: null,
        PreparedById: null,
        Remarks: null,
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    //$scope.GetSequence = function () {
    //    cboService.getSequence($scope.getSeqUrl, function (data) {
    //        $scope.ModelTemp.Sequence = data;
    //        $scope.ModelNew.Sequence = data;
    //    });
    //};
    //$scope.GetSequence();

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        $scope.GetDetailData();
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
                    $scope.ModelNew = response.data.Data;
                    ClearFields();
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
                    ClearFields();
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }

    // Prepared By

    $scope.EmployeeResPersonList = [];
    $scope.ResPersonPopUp = function () {
        angular.element(document.querySelector("#EmployeePopUpResPerson")).modal("show");
        $scope.getEmpData();

    }
    $scope.getEmpData = function () {
        $scope.EmployeeResPersonList = [];
        $http({
            method: 'POST',
            data: { Id: $scope.ModelNew.Id },
            url: $scope.path + 'LoadAllEmpDetails'
        }).then(function successCallback(response) {
            $scope.EmployeeResPersonList = response.data;
        });
    }
    $scope.setEmpData = function (obj) {

        var data = obj.data;
        $scope.ModelNew.EmployeeCode = data.Code;
        $scope.ModelNew.PreparedById = data.Id;
        $scope.ModelNew.ResponsiblePerson = data.EmployeeName;
        angular.element(document.querySelector('#EmployeePopUpResPerson')).modal('hide');
    };
    $scope.ResPersonClear = function () {
        $scope.ModelNew.PreparedById = null;
        $scope.ModelNew.ResponsiblePerson = null;
        $scope.ModelNew.EmployeeCode = null;
        $scope.ModelNew.EmployeeStatus = null;

    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.detailPopUp = function () {
    //    $scope.detailModel = Object.assign({}, $scope.detailTempModel);

        angular.element(document.querySelector('#detailPopUp')).modal('show');
        $scope.GetSequence();
        $scope.GetDependentProcess();
   //     $scope.GetJWActivityListByPOType();
    };

    $scope.closeDetaiPopUp = function () {
        angular.element(document.querySelector('#detailPopUp')).modal('hide');
        $scope.detailClear();
        $scope.GetDetailData();
    //    $scope.detailModel = Object.assign({}, $scope.detailTempModel);
    };

      // Drop Down
    $scope.ProcessList = [];
    $scope.WorkCentreCategoryGroupList = [];
    $scope.DependantProcessList = [];
    $scope.OutputItemNameList = [];
    $scope.EntryQuantityUOMList = [];
    $scope.OutputItemUOMList = [];
    $scope.InputItemNameList = [];
    $scope.InputUOMList = [];
    $scope.ByProductItemNameList = [];
    $scope.ByProductUOMList = [];

    $http({
        method: 'GET',
        url: $scope.path + 'getProcesslist',
    }).then(function successCallback(response) {
        $scope.ProcessList = response.data;
        });

    $scope.ProcessIdDisplay = [];
    $scope.GetProcessIdDisplay = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetProcessIdDisplay?ProcessId=' + $scope.detailModel.ProcessId,
        }).then(function successCallback(response) {
            $scope.ProcessIdDisplay = response.data;
            if ($scope.ProcessIdDisplay.length > 0) {
                $scope.detailModel.ProcessIddisplay = $scope.ProcessIdDisplay[0].Value;
            }
        });
    }

    $http({
        method: 'GET',
        url: $scope.path + 'getWorkCentreCategoryGrouplist',
    }).then(function successCallback(response) {
        $scope.WorkCentreCategoryGroupList = response.data;
        });

    $scope.GetDependentProcess = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'GET',
                url: $scope.path + 'getDependantProcesslist?MasterId=' + $scope.ModelNew.Id,
            }).then(function successCallback(response) {
                $scope.DependantProcessList = response.data;
            });
        }    
    }
   

    $http({
        method: 'GET',
        url: $scope.path + 'getOutputItemNamelist',
    }).then(function successCallback(response) {
        $scope.OutputItemNameList = response.data;
        });

    $scope.GetEntryQtyUom = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getEntryQuantityUOMList?OutputItenNameId=' + $scope.detailModel.OutputItemNameId,
        }).then(function successCallback(response) {
            $scope.EntryQuantityUOMList = response.data;
        });
    }

    $scope.OutputParameter = [];
    $scope.GetOutputItemParameter = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetOutputItemParameter?OutputItenNameId=' + $scope.detailModel.OutputItemNameId,
        }).then(function successCallback(response) {
            $scope.OutputParameter = response.data;
            if ($scope.OutputParameter.length > 0) {
                $scope.detailModel.OutputItemParameterId = $scope.OutputParameter[0].Text;
                $scope.detailModel.ConversionFactorId = $scope.OutputParameter[0].OutputValue;
            }
        });
    }


    $scope.GetOutputItemUom = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getOutputItemUOMList?OutputItenNameId=' + $scope.detailModel.OutputItemNameId,
        }).then(function successCallback(response) {
            $scope.OutputItemUOMList = response.data;
        });
    }
   

    $http({
        method: 'GET',
        url: $scope.path + 'getInputItemNameList',
    }).then(function successCallback(response) {
        $scope.InputItemNameList = response.data;
    });


    $scope.InputUom = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getInputUOMList?InputItenNameId=' + $scope.detailModel.InputItemNameId,
        }).then(function successCallback(response) {
            $scope.InputUOMList = response.data;
        });
    }
  

    $http({
        method: 'GET',
        url: $scope.path + 'getByProductItemNameList',
    }).then(function successCallback(response) {
        $scope.ByProductItemNameList = response.data;
        });


    $scope.GetByProductUom = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getByProductUOMList?ByProductNameId=' + $scope.detailModel.ByProductItemNameId,
        }).then(function successCallback(response) {
            $scope.ByProductUOMList = response.data;
        });
    }

    $scope.ModelDetailTemp = {
        Id: null,
        ProductionTransformationMasterId: null,
        ProcessId: null,
        WorkCentreCategoryGroupId: null,
        DependantProcessId: null,
        OutputItemNameId: null,
        EntryQuantityUOMId: null,
        OutputItemParameterId: null,
        ConversionFactorId: null,
        OutputItemUOMId: null,
        OutputQuantity: null,
        InputUOMId: null,
        GrossConsumptionPerUnitQuantity: null,
        ByProductItemNameId: null,
        ByProductUOMId: null,
        ByProductQuantity: null,
        ByProductCategory: null,
        InvisibleLossPercentage: null,
        ProductionBookingLevel: null,
        IssueConsumptionBooking: null,
        Remarks: null,
        InputItemNameId: null,
        Sequence: 0,
        DependantProcess:null,
    };
    $scope.detailModel = Object.assign({}, $scope.ModelDetailTemp);

    $scope.GetSeq = [];
    $scope.GetSequence = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetAutoSequence?ProductionBookingId=' + $scope.ModelNew.Id,
        }).then(function successCallback(response) {
            $scope.GetSeq = response.data;
            if ($scope.GetSeq.length > 0) {
                $scope.detailModel.Sequence = $scope.GetSeq[0].Sequence;
            }
        });
    }

    $scope.DetailAction = "Save";

    $scope.detailSave = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.GeneralForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + 'detailSave',
                data: { 'data': $scope.detailModel, 'MasterId': $scope.ModelNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.detailModel = response.data.Data;
                    $scope.detailClear();
                    $scope.GetDetailData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.detailClear = function () {
        $scope.DetailAction = 'Save';
        $scope.detailModel = Object.assign({}, $scope.ModelDetailTemp);
        $scope.GetSequence();
        $scope.GetDependentProcess();
    }

    $scope.SelectedDetailDataList = [];
    $scope.GetDetailData = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetDetailData?ProductionBookingId=' + $scope.ModelNew.Id,
        }).then(function successCallback(response) {
            $scope.SelectedDetailDataList = response.data;
          
        });
    }

    $scope.DelBookingDetails = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'DelBookingDetails?Id=' + $scope.BookingDetailId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");

                $scope.detailClear();
                $scope.GetDetailData();
            }

        });
    }

    $scope.ConfirmDeleteDetail = function (Id) {
        $scope.BookingDetailId = Id;
        angular.element(document.querySelector("#DelBookingDetailsTabPopUp")).modal("show");
    }

    $scope.GetDetailDataToEdit = function (args) {
        angular.element(document.querySelector('#detailPopUp')).modal('show');
        $scope.detailModel = Object.assign({}, args.data);
        $scope.detailModel.DependantProcessId = $scope.detailModel.DependantProcessId;
    //    $scope.GetDependentProcess();
        $scope.GetEntryQtyUom();
        $scope.GetOutputItemUom();
        $scope.InputUom();
        $scope.GetByProductUom();
        $scope.DetailAction = 'Update';

    }
}