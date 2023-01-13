'use strict';
PackingScanDataController.$inject = ['cboService', 'commonMessage','$scope', '$http', '$location', "$rootScope", '$window', "$compile", 'baseService', 'fileReader', "$controller", "$filter"];
function PackingScanDataController(cboService,commonMessage,$scope, $http, $location, $rootScope, $window, $compile, baseService, fileReader, $controller, $filter) {
    $scope.path = 'Productions/PackingScanData/';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $rootScope.title = 'Packing Scan Data Upload';
    $controller("bankBaseController", { $scope: $scope, $http: $http });
    $controller("employeeBaseController", { $scope: $scope, $http: $http });
    $scope.SaveDataList = []
    $scope.Action = 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.saveUrl = $scope.path + 'Save';


    $scope.tabh = 11;
    $scope.setTab11 = function (newTab) {
        $scope.tabh = newTab;
        $scope.employees = [];

    };
    $scope.isSet11 = function (tabNum) {
        return $scope.tabh === tabNum;
    };
    $scope.setTab22 = function (newTab) {
        $scope.tabh = newTab;

    };
    $scope.isSet22 = function (tabNum) {
        return $scope.tabh === tabNum;
    };

    $scope.packingscan = {
        Id: null,
        WorkDate: null,
        Time: null,
        ShiftId: null,
        Grade: null,
        LocMasterId: null,
        PurposeId: null,
        Remarks: null,
        //EntityId: null,
        //From: null,
        //To: null,
    };
    $scope.PSDataNew = Object.assign({}, $scope.packingscan);

    $scope.gradeList = [];
    $scope.gradeList = [
        {
            'Value': 'A',
            'Text': 'A'
        },
        {
            'Value': 'B',
            'Text': 'B'
        },
        {
            'Value': 'C',
            'Text': 'C'
        }
    ];
    //#Region start

    //$scope.entityList = [];
    //$scope.getAllEntities = function () {
    //    $http({
    //        method: 'POST',
    //        url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
    //    }).then(function successCallback(response) {
    //        $scope.entityList = response.data;
    //    });
    //}
    //$scope.getAllEntities();

    $scope.purposeList = [];
    $scope.getPurpose = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetPurpose',
        }).then(function successCallback(response) {
            $scope.purposeList = response.data;
        });
    }
    $scope.getPurpose();

    $scope.materialMovementList = [];
    $scope.getMaterialMovementList = function (PurposeId) {
        $http({
            method: 'POST',
            url: $scope.path + 'GetMaterialMovementList',
            data: { 'purposeId': PurposeId},
        }).then(function successCallback(response) {
            $scope.materialMovementList = response.data;

        });
    }
    $scope.getMaterialMovementList();

    $scope.shiftList = [];
    $scope.GetShiftList = function () {
        $http.get('Productions/PackingScanData/GetShiftList')
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.shiftList = response.data;
                    if (baseService.arrayLength(response.data) === 1) {
                        $scope.ShiftId = $scope.shiftList[0].Value;
                    }
                }
            });
    }
    $scope.GetShiftList();

    $scope.SavePCD = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.PSDataNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    //$scope.Clear = function () {
    //    $scope.PSDataNew = Object.assign({}, $scope.packingscan);
    //    $scope.Action = 'Save';
    //};

    $scope.GetSampleFile = function () {
        var ReportFormat = 'Excel';
        location.href = $scope.path + 'GetSampleFile?reportFormat=' + ReportFormat;
    };


    $scope.packingData = {
        Id: null,
        MasterId: null,
        ProductCode: null,
        POId: null,
        LotNo: null,
        RefNo: null,
        Cones: null,
        NetWeight: null,
        GWeight: null,
        PackedBy: null,
        Shade: null,
        Booked: null,
        PackingId: null,
        AddedBy: null,
        AddedDate: null,
        UpdatedBy: null,
        UpdatedDate: null,
        LocMasterId: null,
        IsDespatch: null,
        BookedDate: null,
        InventoryReceiveDetailId: null,
        SalesId: null,
        //ToDate: $filter("dateFiltering")(Date.now())
    };
    $scope.packingNew = Object.assign({}, $scope.packingData);

    $scope.PackingScanUploadedData = [];
    $scope.ImportData = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            var picData = new FormData();
            if (!baseService.isUndefinedOrNull($scope.picdata)) {
                $scope.packingNew.FileName = $scope.picdata.name;
            }
            $http({
                method: 'POST',
                url: $scope.path + 'ImportData',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    picData.append("modelNew", angular.toJson(data.modelNew));
                    if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                        picData.append('file', data.file);
                    }
                    return picData;
                },
                data: {
                    'modelNew': $scope.packingNew
                    , 'file': $scope.picdata
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.ShowSaveBtn = false;
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    $scope.PackingScanUploadedData = [];
                    $scope.PackingScanUploadedData = response.data;
                    $scope.ShowSaveBtn = true;
                }
            }, function errorCallback(response) {

            });
            return true;
        }
        catch (e) {
            ShowResult(e, "failure");
        }
    };

    //End
    $scope.LoadData = function () {
        try {
            $http({
                method: 'GET',
                url: $scope.path + 'LoadBankReconciliationUploadedData?bankMasterId=' + $scope.PSDataNew.BankMasterId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    $scope.SaveDataList = [];
                    $scope.SaveDataList = response.data;
                }
            }, function errorCallback(response) {
            });
            return true;
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    
    $scope.picdata = null;
    $scope.ShowSaveBtn = false;
    $("#uploadImage").change(function () {
        $scope.picdata = this.files[0];
    });

    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };
    $scope.ShowSaveBtn = false;


    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {
        var msg = "";
        if (new Date($scope.PSDataNew.ToDate) < new Date($scope.PSDataNew.FromDate)) {
            msg = "To date must be below or equal to From Date!";
            $scope.invalidDocDate = true;
        }
        else $scope.invalidDocDate = false;

        return manualValidation("div_ToDate", $scope.invalidDocDate, msg);
    };

    $scope.save = function () {
        try {
            $.ajax({
                type: "POST",
                url: $scope.path + 'SavePackingScanUploadData',
                data: {
                    'packingScanUploadvm': $scope.PSDataNew
                    , 'packingScanUploadedDataList': $scope.PackingScanUploadedData
                },
                dataType: "json",
                success: function (response) {
                    if (response.Error === true) {
                        $scope.ShowSaveBtn = true;
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        ShowResult(response.Message, 'success');
                        //$scope.LoadData();
                        $scope.PackingScanUploadedData = [];
                        $scope.PSDataNew = {};
                        $("#uploadImage").val(null);
                        $scope.ShowSaveBtn = false;
                    }
                }
            });
        } catch (e) {
            $scope.ShowSaveBtn = false;
            ShowResult(e, 'failure');
        }
    };

    $scope.onClickReportDownloadExcel = function (data) {
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open($scope.path + 'GetBankReconciliationUploadedDataReport?reportFormat=' + reportFormat + '&bankReconciliationUploadId=' + data.Id, '_blank');
    };
    $scope.onClickDeletePopUp = function (x) {
        var data = x;
        $scope.bankReconciliationUploadId = data.Id;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector('#confirmDeletePopUp')).modal('show');
    };
    $scope.delete = function (bankReconciliationUploadId) {
        $http({
            method: "POST",
            url: $scope.path + 'DeleteBankReconciliationUploadedData',
            data: {
                "bankReconciliationUploadId": bankReconciliationUploadId
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.LoadData();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };
}





