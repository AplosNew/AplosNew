'use strict';
FabricGroupingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function FabricGroupingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Fabric Grouping';
    $scope.Action = 'Save';
    $scope.fabricPendingDetailList = [];
    $scope.path = 'Materials/FabricRoll/';

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.getPendingData = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetFabricRollMaster",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.fabricPendingDetailList = response.data;
        });
    }
    $scope.getPendingData();

    $scope.fabricConfirmDetailList = [];
    $scope.getConfirmData = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetFabricRollMasterConfirmDataList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.fabricConfirmDetailList = response.data;
        });
    }


    $scope.fabricPendingChildListList = [];
    $scope.GetFabricRollChildList = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetFabricRollChildPendingList?FabricRollManagementMasterId=" + $scope.masterId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.fabricPendingChildListList = response.data;
        });
    }

    $scope.fabricConfirmChildListList = [];
    $scope.GetFabricRollConfirmChildList = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetFabricRollChildConfirmList?FabricRollManagementMasterId=" + $scope.masterId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.fabricConfirmChildListList = response.data;
        });
    }

    $scope.Get = function (args) {
        $scope.fabricRollMaster = Object.assign({}, args.data);
        $scope.masterId = args.data.Id;

        $scope.GetFabricRollChildList();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.GetConfirmData = function (args) {
        $scope.fabricRollMaster = Object.assign({}, args.data);
        $scope.masterId = args.data.Id;

        $scope.GetFabricRollConfirmChildList();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.ShowModal = function () {
        try {
            var getRow = $filter("filter")($scope.fabricPendingChildListList, { "Flag": true });
            if (getRow.length === 0)
                throw "Select data.";

            angular.element(document.querySelector('#entryPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SetDataEntry = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.Model.CutableWidth)) {
                throw "CutableWidth is required.";
            }
            if (baseService.isUndefinedOrNull($scope.Model.Shade)) {
                throw "Shade is required.";
            }
            if (baseService.isUndefinedOrNull($scope.Model.ShrinkageLengthWise)) {
                throw "ShrinkageLengthWise is required.";
            }
            if (baseService.isUndefinedOrNull($scope.Model.ShrinkageWidthWise)) {
                throw "ShrinkageWidthWise is required.";
            }
            for (var i = 0; i < $scope.fabricPendingChildListList.length; i++) {
                if ($scope.fabricPendingChildListList[i].Flag) {
                    $scope.fabricPendingChildListList[i].Color = $scope.Model.Color;
                    $scope.fabricPendingChildListList[i].LotNo = $scope.Model.LotNo;
                    $scope.fabricPendingChildListList[i].FabricType = $scope.Model.FabricType;
                    $scope.fabricPendingChildListList[i].FabricQuality = $scope.Model.FabricQuality;
                    $scope.fabricPendingChildListList[i].SupplierRollNo = $scope.Model.SupplierRollNo;
                    $scope.fabricPendingChildListList[i].OwnRollNo = $scope.Model.OwnRollNo;
                    $scope.fabricPendingChildListList[i].SupplierQty = $scope.Model.SupplierQty;
                    $scope.fabricPendingChildListList[i].ActualQty = $scope.Model.ActualQty;
                    $scope.fabricPendingChildListList[i].CutableWidth = $scope.Model.CutableWidth;
                    $scope.fabricPendingChildListList[i].OwnGSM = $scope.Model.OwnGSM;
                    $scope.fabricPendingChildListList[i].StdGSM = $scope.Model.StdGSM;
                    $scope.fabricPendingChildListList[i].GSMVariation = $scope.Model.GSMVariation;
                    $scope.fabricPendingChildListList[i].GSMVariationPer = $scope.Model.GSMVariationPer;
                    $scope.fabricPendingChildListList[i].Shade = $scope.Model.Shade;
                    $scope.fabricPendingChildListList[i].ShrinkageLengthWise = $scope.Model.ShrinkageLengthWise;
                    $scope.fabricPendingChildListList[i].ShrinkageWidthWise = $scope.Model.ShrinkageWidthWise;
                    $scope.fabricPendingChildListList[i].Dia = $scope.Model.Dia;
                    $scope.fabricPendingChildListList[i].SupplierQualityGrade = $scope.Model.SupplierQualityGrade;
                    $scope.fabricPendingChildListList[i].QualityStatus = $scope.Model.QualityStatus;
                    $scope.fabricPendingChildListList[i].FTPReportNo = $scope.Model.FTPReportNo;
                    $scope.fabricPendingChildListList[i].FTPReceiveDate = $scope.Model.FTPReceiveDate;
                    $scope.fabricPendingChildListList[i].FTPStatus = $scope.Model.FTPStatus
                }
            }
            $scope.CloseModal();
            var gridObj = $("#GridPF").data("ejGrid");
            gridObj.refreshContent();
            gridObj.refreshTemplate();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.CloseModal = function () {
        angular.element(document.querySelector('#entryPopUp')).modal('hide');
    }


    $scope.SaveRollData = function () {
        try {
            $http({
                method: "POST",
                url: 'Materials/FabricRoll/CreateFabricRollManage',
                data: {
                    "data": $scope.fabricRollMaster
                    , "grnDetailList": $scope.fabricPendingChildListList
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    if (!$rootScope.isCollapsed) {
                        $rootScope.toggle();
                    }
                    $scope.fabricRollMaster = {};
                    $scope.fabricPendingChildListList = [];
                    $scope.getPendingData();
                    $scope.GetFabricRollChildList();
                    $scope.getConfirmData();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.SaveConfirmData = function () {
        try {
            $scope.fabricRollMaster.IsConfirm = true;
            $http({
                method: "POST",
                url: 'Materials/FabricRoll/CreateFabricRollManage',
                data: {
                    "data": $scope.fabricRollMaster
                    , "grnDetailList": $scope.fabricConfirmChildListList
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    if (!$rootScope.isCollapsed) {
                        $rootScope.toggle();
                    }
                    $scope.fabricRollMaster = {};
                    $scope.fabricConfirmChildListList = [];
                    $scope.getPendingData();
                    $scope.GetFabricRollChildList();
                    $scope.getConfirmData();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.exportgriddataUrlUpdate2 = 'GridReports/ExcelExportUpdate2';
    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.GetReport = function () {
        var dataList = [];
        var g = $("#GridPF").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.fabricPendingChildListList;
        }
        var ob = {};
        var tempList = [];
        for (var i = 0; i < dataList.length; i++) {
            ob.Id = dataList[i].Id;
            ob.FabricRollManagementMasterId = dataList[i].FabricRollManagementMasterId;
            ob.Sequence = dataList[i].Sequence;
            ob.GRNRowId = dataList[i].GRNRowId;
            ob.Color = dataList[i].Color;
            ob.LotNo = dataList[i].LotNo;
            ob.FabricType = dataList[i].FabricType;
            ob.FabricQuality = dataList[i].FabricQuality;
            ob.SupplierRollNo = dataList[i].SupplierRollNo;
            ob.OwnRollNo = dataList[i].OwnRollNo;
            ob.QtyUoM = dataList[i].QtyUoM;
            ob.SupplierQty = dataList[i].SupplierQty;
            ob.ActualQty = dataList[i].ActualQty;
            ob.CutableWidth = dataList[i].CutableWidth;
            ob.OwnGSM = dataList[i].OwnGSM;
            ob.StdGSM = dataList[i].StdGSM;
            ob.GSMVariation = dataList[i].GSMVariation;
            ob.GSMVariationPer = dataList[i].GSMVariationPer;
            ob.Shade = dataList[i].Shade;
            ob.ShrinkageLengthWise = dataList[i].ShrinkageLengthWise;
            ob.ShrinkageWidthWise = dataList[i].ShrinkageWidthWise;
            ob.Dia = dataList[i].Dia;
            ob.SupplierQualityGrade = dataList[i].SupplierQualityGrade;
            ob.QualityStatus = dataList[i].QualityStatus;
            ob.FTPReportNo = dataList[i].FTPReportNo;
            ob.FTPReceiveDate = dataList[i].FTPReceiveDate;
            ob.FTPStatus = dataList[i].FTPStatus;

            tempList.push(ob);
            ob = {};
        }


        $scope.fileName = $scope.fabricRollMaster.GRNNo + '-' + "FabricRollReport";
        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: {
                'reportFileName': $scope.fileName,
                'data': tempList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };

    $scope.picdata = null;
    $scope.ShowSaveBtn = false;
    $("#uploadImage").change(function () {
        $scope.picdata = this.files[0];
    });

    $scope.ModelNew = { FileName: null };
    $scope.ImportData = function () {
        try {
            $scope.msg = "";

            var picData = new FormData();
            if (!baseService.isUndefinedOrNull($scope.picdata)) {
                $scope.ModelNew.FileName = $scope.picdata.name;
            }

            $http({
                method: 'POST',
                url: 'Materials/FabricRoll/ImportFabricData',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    picData.append("modelNew", angular.toJson(data.modelNew));
                    if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                        picData.append('file', data.file);
                    }
                    return picData;
                },
                data: { 'modelNew': $scope.ModelNew, 'file': $scope.picdata }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");

                }
                else {
                    $scope.fabricPendingChildListList = [];
                    for (var i = 0; i < response.data.length; i++) {
                        $scope.fabricPendingChildListList.push(response.data[i]);
                    }
                    var gridObj = $("#GridPF").data("ejGrid");
                    gridObj.refreshContent();
                    gridObj.refreshTemplate();
                }
            }, function errorCallback(response) {

            });
            return true;


        } catch (e) {

            ShowResult(e, "failure");
        }
    };

    $scope.GroupingDataList = [];
    $scope.getGroupingData = function () {
        $http({
            method: 'GET',
            url: $scope.path + "getGroupingData",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.GroupingDataList = response.data;
            for (var i = 0; i < $scope.GroupingDataList.length; i++) {
                $scope.GroupingDataList[i].MarkerGroup = $scope.GroupingDataList[i].CutableWidthGroup + "/" + $scope.GroupingDataList[i].ShrinkageWidthGroup + "/" + $scope.GroupingDataList[i].ShrinkageLengthGroup;
                $scope.GroupingDataList[i].Remarks = null;
            }
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        });
    }

    function containsSpecialChars(str) {
        const specialChars = /[`!@#$%^&*()_+\=\[\]{};':"\\|,.<>\/?~]/;
        return specialChars.test(str);
    }

    $scope.CheckSpecialCharecter = function (data) {
        try {
            var objt = data.data;
          
            if (containsSpecialChars(objt.ShadeGroup)) {
                objt.ShadeGroup = objt.ShadeGroup.substring(0, objt.ShadeGroup.length - 1);
                throw "No special characters allowed for Shade Group.";
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.MakeMarkerGroup = function (data) {
        var objt = data.data;
        objt.MarkerGroup = objt.CutableWidthGroup + "/" + objt.ShrinkageWidthGroup + "/" + objt.ShrinkageLengthGroup;
        //var gridObj = $("#GridGD").data("ejGrid");
        //gridObj.refreshContent();
        //gridObj.refreshTemplate();
    }

    $scope.SaveGrouingData = function () {
        try {
            $http({
                method: "POST",
                url: 'Materials/FabricRoll/CreateFabricGrouping',
                data: {
                    "grnDetailList": $scope.GroupingDataList
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.GroupingDataList = [];
                    $scope.getGroupingData();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
        } catch (e) {
            ShowResult(e, "failure");
        }
    };




}
