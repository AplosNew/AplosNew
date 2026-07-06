'use strict';
bulletinTemplateController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function bulletinTemplateController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Bulletin Template';
    $scope.Action = 'Save';
    $scope.ProcessAction = 'Save';
    $scope.BuyerAction = 'Save';
    $scope.index = -1;
    $scope.bulletinMasters = [];
    $scope.path = 'IE/bulletintemplate/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveProcessUrl = $scope.path + 'createprocess';
    $scope.saveBuyerUrl = $scope.path + 'createbuyer';
    $scope.saveOperationUrl = $scope.path + 'createoperation';
    $scope.saveMachineUrl = $scope.path + 'updatemachine';
    $scope.saveSeqUrl = $scope.path + 'updatesequence';
    $scope.UpdateOperationVaiationCodeUrl = $scope.path + 'UpdateOperationVaiationCode';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'Delete/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.copyUrl = $scope.path + 'copy/';

    $scope.IsMachineChangeableinBulletinTemplate = false;
    $scope.GetMachineChangeInfo = function () {
        $http.get('IE/bulletintemplate/GetMachineChangeInfo?plantId=' + $window.plantId)
            .then(function (response) {
                $scope.IsMachineChangeableinBulletinTemplate = response.data[0].IsMachineChangeableinBulletinTemplate;
            });
    };
    $scope.GetMachineChangeInfo();

    //#region getData
    $scope.getData = function () {
        $scope.bulletinMasters = [];
        $http({
            method: 'GET',
            url: $scope.getListUrl
        }).then(function successCallback(response) {
            $scope.bulletinMasters = response.data;
        });
    }
    $scope.getData();

    //#endregion

    //#region Object

    $scope.bulletinTemplate = {
        Id: null,
        CompanyGroupId: null,
        BulletinName: null,
        AlternativeName: null,
        ByWhom: null,
        ProductMasterId: null,
        SizeGroupId: null,
        PBCount: 0
    };
    $scope.bulletinTemplateNew = Object.assign({}, $scope.bulletinTemplate);

    $scope.bulletinProcess = {
        Id: null,
        BulletinTemplateId: null,
        ProcessId: null,
        RequiredStdTarget: null,
        MaxNoOfWS: null,
        PlannedHoursPerDay: null,
        BottleNeckPercentage: null
    }
    $scope.bulletinProcessNew = Object.assign({}, $scope.bulletinProcess);

    $scope.bulletinBuyer = {
        Id: null,
        BulletinTemplateId: null,
        BuyerId: null,
        BuyerStyleRefNo: null,
        OwnStyleRefNo: null
    }
    $scope.bulletinBuyerNew = Object.assign({}, $scope.bulletinBuyer);

    $scope.bulletinTemplateDetail = {
        Id: null,
        BulletinTemplateMasterId: null,
        Sequence: 0,
        OperationVariationId: null,
        OperationGroup: null,
        SkillId: null,
        MachineVarientId: null,
        FGZoneId: null,
        FGComponentId: null,
        AdditionalSPT: null,
        TotalSPT: 0,
        AllotedWorkstation: 0,
        AllotedManpower: 0,
        AttachmentId: null,
        GaugeFolderId: null,
        OperationConsumptionId: null,
        OperationTypeId: null,
        Frequency: 0,
        Remark: null,
        OperationCategoryId: null,
        QualityLevel: null,
        AvgAllotedTime: 0,
        OperationTargetPerHr: 0,
        RequiredManPower: 0,
        SPI: 0,
        NoOfStitch: 0,
        OperationLength: 0,
        StitchCodeId: null,
        FabricWidth: 0,
        Needle: 0,
        NeedleMaterialMasterId: null,
        NeedleArticleId: null,
        NeedleMaterialMaster: null,
        NeedleArticle: null,
        Bobbin: 0,
        BobbinMaterialMasterId: null,
        BobbinArticleId: null,
        BobbinMaterialMaster: null,
        BobbinArticle: null,
        Looper: 0,
        LooperMaterialMasterId: null,
        LooperArticleId: null,
        LooperMaterialMaster: null,
        LooperArticle: null,
        OperationCode: null
    }
    $scope.bulletinTemplateDetailNew = Object.assign({}, $scope.bulletinTemplateDetail);

    //#endregion

    // #region Cbo

    $scope.OperationVariationList = [];
    cboService.getOperationVariationCbo(function (response) {
        $scope.OperationVariationList = response;
    });

    $scope.OperationTypeList = [];
    cboService.getOperationTypeCbo(function (response) {
        $scope.OperationTypeList = response;
    });

    $scope.OperationConsumptionList = [];
    cboService.getOperationConsumptionCbo(function (response) {
        $scope.OperationConsumptionList = response;
    });

    $scope.OperationCategoryList = [];
    cboService.getOperationCategoryCbo(function (response) {
        $scope.OperationCategoryList = response;
    });

    $scope.MachineVariantList = [];
    cboService.getMachineVariantCbo(function (response) {
        $scope.MachineVariantList = response;
    });

    $scope.FGZoneList = [];
    cboService.getFGZoneCbo(function (response) {
        $scope.FGZoneList = response;
    });

    $scope.FGComponentList = [];
    cboService.getFGComponentCbo(function (response) {
        $scope.FGComponentList = response;
    });

    $scope.gaugeFolderList = [];
    cboService.getGaugeFolderCbo(function (response) {
        $scope.gaugeFolderList = response;
    });

    $scope.attachmentList = [];
    cboService.getAttachmentCbo(function (response) {
        $scope.attachmentList = response;
    });

    $scope.sizeGroupList = [];
    cboService.getSizeGroupCbo(function (response) {
        $scope.sizeGroupList = response;
    });

    $scope.productMasterList = [];
    cboService.getProductMasterCbo(function (response) {
        $scope.productMasterList = response.Rows;
    });

    //$scope.processCboList = [];
    //cboService.getProcessCbo(function (response) {
    //    $scope.processCboList = response;
    //});

    $scope.processCboList = [];
    cboService.getProductionProcessCbo(function (response) {
        $scope.processCboList = response;
    });

    $scope.buyerCboList = [];
    cboService.getCboBuyer(function (response) {
        $scope.buyerCboList = response;
    });

    $scope.machineCboList = [];
    function getMachine(processId) {
        cboService.getMachineCbo(processId, function (response) {
            $scope.machineCboList = response;
        });
    }

    $scope.stitchCodeList = [];
    $http.get('Machines/StitchCode/GetCbo')
        .then(function (response) {
            $scope.stitchCodeList = response.data;
        });

    // #endregion

    // #region bulletinTemplate

    $scope.Get = function (obj) {
        $scope.TotalSPT = 0;
        $scope.TotalWorkStation = 0;
        $scope.TotalManpower = 0;
        $scope.OrganizationEfficiency = 0;
        $scope.ProductionEfficiencyPerHour = 0;
        $scope.TotalManpower = 0;
        $scope.PitchTime = 0;
        $scope.ProductionEfficiencyPerDay = 0;
        $scope.MaxAllottedTime = 0;
        $scope.LineTargetPerHour = 0;
        $scope.MCtotalspt = 0;
        $scope.NonMCtotalspt = 0;

        $scope.TotalMP = 0;
        $scope.MCtotalMPt = 0;
        $scope.NonMCtotalMP = 0;

        $scope.processList = [];
        $scope.buyerList = [];
        $scope.operationList = [];
        $scope.bulletinTemplate = obj.data;
        $scope.bulletinTemplateNew = Object.assign({}, $scope.bulletinTemplate);

        if (!baseService.isUndefinedOrNull($scope.bulletinTemplateNew.PicFileName)) {
            var str = $scope.bulletinTemplateNew.PicFileName;
            var extention = str.substr(str.indexOf('.'));
            $scope.PicFileName = virtualPath.BulletinTemplateImage + '/' + $scope.bulletinTemplateNew.Id + extention;
        }

        $scope.getProcessData($scope.bulletinTemplateNew.Id);
        $scope.getBuyerData($scope.bulletinTemplateNew.Id);
        $scope.Action = 'Update';

        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        angular.copy($scope.bulletinTemplateNew, $scope.bulletinTemplate);
        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.BulletinForm.$valid) {
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.bulletinTemplate,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');

                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.bulletinMasters.push(response.data.BulletinTemplate);
                            $scope.bulletinTemplateNew.Id = response.data.BulletinTemplate.Id;
                            $scope.getData();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');

                    };
                }
                else if ($scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: $scope.bulletinTemplate,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');

                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            if ($scope.index > -1) {
                                $scope.bulletinMasters[$scope.index] = $scope.bulletinTemplate;
                                //$scope.bulletinTemplateNew.Id = response.data.BulletinTemplate.Id;
                            }
                            $scope.getData();
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');

                    });
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.bulletinTemplateNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.bulletinTemplateNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Copy = function () {
        if (!baseService.isUndefinedOrNull($scope.bulletinTemplateNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.copyUrl,
                data: $scope.bulletinTemplateNew,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                    $scope.getData();
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
        $scope.bulletinTemplate = {};
        $scope.bulletinTemplateNew = Object.assign({}, $scope.bulletinTemplate);
        $scope.processList = [];
        $scope.buyerList = [];
        $scope.operationList = [];
        $scope.TotalSPT = 0;
        $scope.TotalWorkStation = 0;
        $scope.TotalManpower = 0;
        $scope.OrganizationEfficiency = 0;
        $scope.ProductionEfficiencyPerHour = 0;
        $scope.TotalManpower = 0;
        $scope.PitchTime = 0;
        $scope.ProductionEfficiencyPerDay = 0;
        $scope.MaxAllottedTime = 0;
        $scope.LineTargetPerHour = 0;
        $scope.MCtotalspt = 0;
        $scope.NonMCtotalspt = 0;
        $scope.MCtotalMP = 0;
        $scope.TotalMP = 0;
        $scope.MCtotalMPt = 0;
        $scope.NonMCtotalMP = 0;
        $scope.machineOperationList = [];
        $scope.BulletinTemplateMasterId = null;
        $scope.PicFileName = virtualPath.BulletinTemplateImage + '';
        $scope.bulletinTemplateNew.PBCount = 0;
    }

    // #endregion bulletinTemplate

    // #region Process
    $window.onresize = function (event) {
        $scope.actionCompleteUnassign();
    };
    $scope.actionCompleteUnassign = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#Grid").ejGrid("instance");
                var scrollerwidth = $("#ProcessH").width();//Obtain the width of the container

                $("#GridProcess").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 100 } });
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };

    $scope.SaveProcess = function () {
        $scope.bulletinProcess.BulletinTemplateId = $scope.bulletinTemplateNew.Id;
        $scope.bulletinProcessNew.BulletinTemplateId = $scope.bulletinTemplateNew.Id;
        angular.copy($scope.bulletinProcessNew, $scope.bulletinProcess);
        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.ProcessForm.$valid) {
                if ($scope.ProcessAction === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveProcessUrl,
                        data: $scope.bulletinProcess,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure', 'ProcessPoUp');
                        }
                        else {
                            ShowResult(response.data.Message, 'success', 'ProcessPoUp');
                            $scope.getProcessData($scope.bulletinTemplateNew.Id);
                            angular.element(document.querySelector('#ProcessPoUp')).modal('hide');
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'ProcessPoUp');

                    };
                }
                else if ($scope.ProcessAction === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.saveProcessUrl,
                        data: $scope.bulletinProcess,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure', 'ProcessPoUp');
                        }
                        else {
                            ShowResult(response.data.Message, 'success', 'ProcessPoUp');
                            $scope.getProcessData($scope.bulletinTemplateNew.Id);
                            angular.element(document.querySelector('#ProcessPoUp')).modal('hide');
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'ProcessPoUp');
                    });
                }
            }
        } catch (e) {
            ShowResult(e, 'failure', 'ProcessPoUp');
        }
    };

    $scope.getProcessQtyAndNoWSData = function () {
        $http({
            method: 'GET',
            url: 'ie/bulletintemplate/getprocessqtyandnowsdata?processId=' + $scope.bulletinProcessNew.ProcessId + '&productMasterId=' + $scope.bulletinTemplateNew.ProductMasterId
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.bulletinProcessNew.RequiredStdTarget = response.data[0].TargetQty;
                $scope.bulletinProcessNew.MaxNoOfWS = response.data[0].NoOfWorkStation;
            }
        });
    }

    $scope.processList = [];
    $scope.getProcessData = function (bulletinTemplateId) {
        $scope.processList = [];
        $http({
            method: 'GET',
            url: 'ie/bulletintemplate/getprocessdata?bulletinTemplateId=' + bulletinTemplateId
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.processList = response.data;

                $scope.Process = $scope.processList[0].Process;
                $scope.ProcessId = $scope.processList[0].ProcessId;
                $scope.BulletinTemplateMasterId = $scope.processList[0].Id;
                $scope.PlannedHoursPerDay = $scope.processList[0].PlannedHoursPerDay;
                $scope.RequiredStdTarget = $scope.processList[0].RequiredStdTarget;

                $scope.getSavedOperationData($scope.processList[0].Id);
                var gridObj = $("#GridProcess").data("ejGrid");
                gridObj.refreshContent();
                gridObj.refreshTemplate();
            }
        });
    }

    $scope.AddNewProcess = function () {
        $scope.bulletinProcess = {};
        $scope.bulletinProcessNew = {};
        angular.element(document.querySelector('#ProcessPoUp')).modal('show');
    }
    $scope.CloseProcess = function () {
        angular.element(document.querySelector('#ProcessPoUp')).modal('hide');
    }

    $scope.EditProcess = function (obj) {
        var gridObj = $("#GridProcess").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.bulletinProcessNew = data;
        $scope.Process = $scope.bulletinProcessNew.Process;
        $scope.ProcessAction = 'Update';
        angular.element(document.querySelector('#ProcessPoUp')).modal('show');
    }

    $scope.getProcess = function (obj) {
        var gridObj = $("#GridProcess").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.bulletinProcessNew = data;
        $scope.Process = $scope.bulletinProcessNew.Process;
        $scope.MaxNoOfWS = $scope.bulletinProcessNew.MaxNoOfWS;
        $scope.bulletinTemplateDetailNew.BulletinTemplateMasterId = $scope.bulletinProcessNew.Id;
        $scope.BulletinTemplateMasterId = $scope.bulletinProcessNew.Id;
        $scope.ProcessId = $scope.bulletinProcessNew.ProcessId;
        $scope.PlannedHoursPerDay = $scope.bulletinProcessNew.PlannedHoursPerDay;
        $scope.RequiredStdTarget = $scope.bulletinProcessNew.RequiredStdTarget;
        $scope.getSavedOperationData($scope.BulletinTemplateMasterId);
        $scope.getSavedMacnineOperationData($scope.BulletinTemplateMasterId);
    }

    $scope.message_confirmation = null;
    $scope.removeProcess = function (obj) {
        var gridObj = $("#GridProcess").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.bulletinProcessNew = data;
        if (!baseService.isUndefinedOrNull($scope.bulletinProcessNew.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently [ ' + $scope.bulletinProcessNew.Process + ' ]';
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
    }

    $scope.DeleteProcess = function () {
        $http({
            method: 'POST',
            url: 'IE/BulletinTemplate/DeleteProcess?id=' + $scope.bulletinProcessNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.processList = [];
                $scope.getProcessData($scope.bulletinTemplateNew.Id);
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    // #endregion

    // #region Buyer

    $scope.buyerList = [];
    $scope.getBuyerData = function (bulletinTemplateId) {
        $scope.buyerList = [];
        $http({
            method: 'GET',
            url: 'ie/bulletintemplate/getbuyerdata?bulletinTemplateId=' + bulletinTemplateId
        }).then(function successCallback(response) {
            $scope.buyerList = response.data;
        });
    }

    $scope.SaveBuyer = function () {
        $scope.bulletinBuyer.BulletinTemplateId = $scope.bulletinTemplateNew.Id;
        $scope.bulletinBuyerNew.BulletinTemplateId = $scope.bulletinTemplateNew.Id;
        angular.copy($scope.bulletinBuyerNew, $scope.bulletinBuyer);
        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.BuyerForm.$valid) {
                if ($scope.BuyerAction === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveBuyerUrl,
                        data: $scope.bulletinBuyer,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure', 'BuyerPoUp');
                        }
                        else {
                            ShowResult(response.data.Message, 'success', 'BuyerPoUp');
                            $scope.getBuyerData($scope.bulletinTemplateNew.Id);
                            angular.element(document.querySelector('#BuyerPoUp')).modal('hide');
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'BuyerPoUp');

                    };
                }
                else if ($scope.BuyerAction === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.saveBuyerUrl,
                        data: $scope.bulletinBuyer,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure', 'BuyerPoUp');
                        }
                        else {
                            ShowResult(response.data.Message, 'success', 'BuyerPoUp');
                            $scope.getBuyerData($scope.bulletinTemplateNew.Id);
                            angular.element(document.querySelector('#BuyerPoUp')).modal('hide');
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'BuyerPoUp');
                    });
                }
            }
        } catch (e) {
            ShowResult(e, 'failure', 'BuyerPoUp');
        }
    };

    $scope.EditBuyer = function (obj) {
        var gridObj = $("#GridBuyer").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.bulletinBuyerNew = data;
        $scope.BuyerAction = 'Update';
        angular.element(document.querySelector('#BuyerPoUp')).modal('show');
    }

    $scope.removeBuyer = function (obj) {
        var gridObj = $("#GridBuyer").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.bulletinBuyerNew = data;
        if (!baseService.isUndefinedOrNull($scope.bulletinBuyerNew.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently [' + $scope.bulletinBuyerNew.Buyer + ' ]';
        angular.element(document.querySelector('#confirmBuyerPopUp')).modal('show');
    }

    $scope.DeleteBuyer = function () {
        $http({
            method: 'POST',
            url: 'IE/BulletinTemplate/DeleteBuyer?id=' + $scope.bulletinBuyerNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.buyerList = [];
                $scope.getBuyerData($scope.bulletinTemplateNew.Id);
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    $scope.AddNewBuyer = function () {
        $scope.bulletinBuyer = {};
        $scope.bulletinBuyerNew = {};
        angular.element(document.querySelector('#BuyerPoUp')).modal('show');
    }

    $scope.CloseBuyer = function () {
        angular.element(document.querySelector('#BuyerPoUp')).modal('hide');
    }

    // #endregion

    // #region checkbox all

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridOperation").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.searchdata.length; i++) {
                $scope.searchdata[i].Active = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }


        }
        var gridObj = $("#GridOperation").data("ejGrid");
        gridObj.refreshContent();
    };

    // #endregion checkbox all

    //#region Tab

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    //#endregion

    // #region Operation

    $scope.ShowResultCustom = function (message, type) {
        $("#OperationPoUp").ejDialog("setTitle", "Operation");
        var eDialog = $("#OperationPoUp").data("ejDialog");
        eDialog.open();

        var gridObj = $("#GridOperation").data("ejGrid");
        gridObj.clearFiltering();  // clears all the filtering

    };

    $scope.searchdata = [];
    $scope.GetOperationData = function () {
        $scope.searchdata = [];
        $http({
            method: 'GET',
            url: 'ie/bulletintemplate/getoperationdata?processId=' + $scope.ProcessId + '&bulletinTemplateId=' + $scope.bulletinTemplateNew.Id + '&productMasterId=' + $scope.bulletinTemplateNew.ProductMasterId
        }).then(function successCallback(response) {
            $scope.searchdata = response.data;
        });
    }

    $scope.AddOperation = function () {
        if (baseService.isUndefinedOrNull($scope.Process)) {
            return ShowResult('Select Process.', 'failure');
        }
        $scope.GetOperationData();
        $scope.ShowResultCustom();
    }

    $scope.operationList = [];
    function MakeData() {

        for (var i = 0; i < $scope.searchdata.length; i++) {
            if ($scope.searchdata[i].Active == true) {
                if (checkExists($scope.operationList, $scope.searchdata[i].OperationVariationId) === false) {
                    var ob = {};
                    ob.Id = null;
                    ob.BulletinTemplateMasterId = $scope.BulletinTemplateMasterId;
                    ob.Sequence = null;
                    ob.OperationVariationId = $scope.searchdata[i].OperationVariationId;
                    ob.OperationGroup = null;
                    ob.MachineVarientId = $scope.searchdata[i].MachineVarientId;
                    ob.MaterialMaster = $scope.searchdata[i].MaterialMaster;
                    ob.MachineName = $scope.searchdata[i].Article;
                    ob.SkillMasterId = $scope.searchdata[i].SkillMasterId;
                    ob.FGZoneId = null;
                    ob.FGComponentId = null;
                    ob.Symbol = $scope.searchdata[i].AdditionalSAMSymbol;
                    ob.AdditionalSPT = 0;
                    ob.AvgAllotedTime = 0,
                        ob.VASSAMSOURCE = $scope.searchdata[i].VASSAMSOURCE;
                    ob.TotalSPT = $scope.searchdata[i].TotalSAM;
                    ob.OperationSPT = $scope.searchdata[i].TotalSAM;
                    ob.AllotedWorkstation = 0;
                    ob.AllotedManpower = 0;
                    ob.AttachmentId = null;
                    ob.GaugeFolderId = null;
                    ob.OperationConsumptionId = null;
                    ob.OperationTypeId = $scope.searchdata[i].OperationTypeId;
                    ob.Frequency = $scope.searchdata[i].Frequency;
                    ob.Remark = null;
                    ob.OperationVariation = $scope.searchdata[i].OperationVariation;
                    ob.OperationCode = $scope.searchdata[i].OperationCode;
                    ob.OperationId = $scope.searchdata[i].OperationId;
                    ob.OperationCategoryId = $scope.searchdata[i].OperationCategoryId;
                    ob.AreaCode = $scope.searchdata[i].AreaCode;
                    ob.QualityLevel = null;
                    ob.SPI = $scope.searchdata[i].SPI;
                    ob.StitchCodeId = $scope.searchdata[i].StitchCodeId;
                    ob.NoOfStitch = 1;
                    ob.OperationLength = $scope.searchdata[i].OperationLength;
                    ob.FabricWidth = 0;

                    $scope.operationList.push(ob);
                }
                else {
                    throw "This Operation Variation " + $scope.searchdata[i].OperationVariation + " is already taken.";
                }
            }
        }

    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].OperationVariationId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.CloseOperation = function () {
        try {
            MakeData();
            $scope.SaveOperation();
            var eDialog = $("#OperationPoUp").data("ejDialog");
            eDialog.close();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.ViewOperation = function () {
        angular.element(document.querySelector('#OperationPoUp')).modal('show');
    }

    $scope.getSavedOperationData = function (bulletinTemplateMasterId) {

        if (baseService.isUndefinedOrNull(bulletinTemplateMasterId)) {
            $scope.bulletinTemplateMasterId = $scope.BulletinTemplateMasterId;
        } else {
            $scope.bulletinTemplateMasterId = bulletinTemplateMasterId;
        }

        $http({
            method: 'GET',
            url: 'ie/bulletintemplate/getbulletinoperation?bulletinTemplateMasterId=' + $scope.bulletinTemplateMasterId
        }).then(function successCallback(response) {
            $scope.operationList = response.data;

            $scope.CalculateGroup();
        });

    };

    $scope.ShowCodePOpUp = function () {
        angular.element(document.querySelector('#OperationVaiationCodesPopup')).modal('show');
    }

    $scope.operationCodelist = [];
    $scope.AddCode = function () {
        if (checkCode($scope.operationCodelist, $scope.bulletinTemplateDetailNew.OperationCode) === false) {
            $scope.operationCodelist.push({
                //OperationCode: "'" + $scope.bulletinTemplateDetailNew.OperationCode + "'"
                OperationCode: $scope.bulletinTemplateDetailNew.OperationCode
            });
        } else {
            ShowResult("This code is exists", 'failure', 'OperationVaiationCodesPopup');
        }
        $scope.bulletinTemplateDetailNew.OperationCode = null;
    }

    function checkCode(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].OperationCode === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.machineOperationList = [];
    $scope.getSavedMacnineOperationData = function (bulletinTemplateMasterId) {
        $scope.machineOperationList = [];

        if (baseService.isUndefinedOrNull(bulletinTemplateMasterId)) {
            $scope.bulletinTemplateMasterId = $scope.BulletinTemplateMasterId;
        } else {
            $scope.bulletinTemplateMasterId = bulletinTemplateMasterId;
        }

        if (!baseService.isUndefinedOrNull($scope.bulletinTemplateMasterId)) {
            $http({
                method: 'GET',
                url: 'ie/bulletintemplate/GetBulletinMachineOperation?bulletinTemplateMasterId=' + $scope.bulletinTemplateMasterId
            }).then(function successCallback(response) {
                $scope.machineOperationList = response.data;
                $scope.WastagePercentage = response.data[0].WastagePercentage;
                $scope.ExtraOrderPercentage = response.data[0].ExtraOrderPercentage;
            });
        }

    };

    $scope.getOperationSPTByMachine = function (args) {
        if (!baseService.isUndefinedOrNull(args)) {

            var gridObj = $("#GridOperation").ejGrid("instance");
            var currRow = gridObj.model.currentViewData[this.element.closest("tr").index()];
            var x = args;

            for (var i = 0; i < $scope.machineCboList.length; i++) {
                if ($scope.machineCboList[i].Value === args.selectedValue) {
                    currRow.OperationSPT = $scope.machineCboList[i].OperationSPT;
                }
            }

        }
    }

    // #region  Machine Popup    

    $scope.ActionMachine = 'Save';

    $scope.openMachinePopup = function (args) {
        var gridObj = $("#GridBulOperation").data("ejGrid");
        $scope.data = gridObj.getSelectedRecords()[0];

        $scope.ActionMachine = 'Update';
        $scope.BulletinTemplateDetailId = $scope.data.Id;
        $scope.operationId = $scope.data.OperationId;
        $scope.MachineVarientId = $scope.data.MachineVarientId;
        $scope.SkillId = $scope.data.SkillId;

        $scope.operationVariationNew.ArticleId = $scope.data.MachineVarientId;
        $scope.operationVariationNew.ArticleName = $scope.data.MachineName;
        $scope.operationVariationNew.MaterialName = $scope.data.MaterialMaster;

        $scope.operationVariationNew.SkillId = $scope.data.SkillId;
        $scope.operationVariationNew.SkillName = $scope.data.SkillName;

        $scope.operationVariationNew.BasicProcessTime = $scope.data.BasicProcessTime;
        $scope.operationVariationNew.AssociateProcessTime = $scope.data.AssociateProcessTime;
        $scope.operationVariationNew.PersonalAllowance = $scope.data.PersonalAllowance;
        $scope.operationVariationNew.MachineAllowance = $scope.data.MachineAllowance;
        $scope.operationVariationNew.AdditionalAllowance = $scope.data.AdditionalAllowance;

        $scope.operationVariationNew.Frequency = $scope.data.Frequency;
        $scope.operationVariationNew.SPI = $scope.data.SPI;
        $scope.operationVariationNew.IsMachineRequired = $scope.data.IsMachineRequired;
        $scope.operationVariationNew.TotalSAM = $scope.data.TotalSAM;
        $scope.operationVariationNew.AdditionalSAMSymbol = $scope.data.AdditionalSAMSymbol;
        $scope.operationVariationNew.AdditionalSAM = $scope.data.AdditionalSAM;
        $scope.operationVariationNew.SubOperationSAM = $scope.data.SubOperationSAM;
        $scope.data.OperationSPT = $scope.data.TotalSAM;

        //  getOperationVariationUtilityData($scope.operationId, $scope.MachineVarientId, $scope.SkillId);

        angular.element(document.querySelector('#MachinePopUp')).modal('show');
    }




    $scope.closeMachinePopUp = function () {
        angular.element(document.querySelector('#MachinePopUp')).modal('hide');
    }

    // #endregion      

    // #region Material Master

    $scope.materialList = [];
    $scope.materialParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'MaterialMasterName'
        , searchBy: "MaterialMasterName"
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };

    $scope.tempdata = {};
    $scope.materialPopUp = function () {
        $scope.materialDataList = [];
        $scope.materialUrl = 'Materials/MaterialMaster/GetCommonMachineListByProcess?processIds=' + baseService.getColumnValueList($scope.ProcessId, 'ProcessId');
        baseService.setCurrentPage('materialDataList');
        $scope.getMaterialData = function (pageno) {
            baseService.paginationBase($scope.materialUrl, pageno, $scope.materialParameters)
                .then(function (result) {
                    $scope.materialDataList = result.Rows;
                    $scope.materialParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.materialList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.materialList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'materialId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#materialId')).modal('show');
        $scope.getMaterialData();
    };
    $scope.closeMaterial = function () {
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#materialId')).modal('hide');
    };

    // #endregion MM

    // #region Article

    $scope.articleList = [];
    $scope.articleParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'StandardName'
        , searchBy: "StandardName"
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };

    $scope.articlePopUp = function (materialMasterId, materialMasterName, materialIndex) {
        try {
            var flag = false;

            var opProcessIds = $scope.ProcessId;

            var prosessIds = $scope.materialDataList[materialIndex].ProsessIds;
            if (!baseService.isUndefinedOrNull(prosessIds) && !baseService.isUndefinedOrNull(opProcessIds)) {
                var opProcessArray = opProcessIds.split(',');
                var processAray = prosessIds.split(',');
                for (var i = 0; i < baseService.arrayLength(processAray); i++) {
                    if (opProcessArray.indexOf(processAray[i]) !== -1) {
                        flag = true;
                        break;
                    }
                }
            }
            if (!flag) throw 'operation process and machine process not match ';
            $scope.excluedList = ['SkillName', 'MachineAllowance'];
            $scope.articleDataList = [];
            $scope.articleUrl = 'Machines/operation/GetArticleListByMaterialMaster?materialMasterId=' + materialMasterId;
            baseService.setCurrentPage('dataList');
            $scope.getarticleData = function (pageno) {
                baseService.paginationBase($scope.articleUrl, pageno, $scope.articleParameters)
                    .then(function (result) {
                        $scope.articleDataList = result.Rows;
                        $scope.articleParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.articleList) === 0) {
                            baseService.getDDLSearchColumn(result.Rows, $scope.articleList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure', 'articleId');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#articleId')).modal('show');
            $scope.getarticleData();
        } catch (e) {
            ShowResult(e, '', 'materialId');
        }

    };

    $scope.selectArticle = function (data) {
        $scope.operationVariationNew.ArticleId = data.Id;
        $scope.operationVariationNew.ArticleName = data.StandardName;
        //$scope.operationVariationNew.SkillId = data.SkillId;
        //$scope.operationVariationNew.SkillName = data.SkillName;
        $scope.operationVariationNew.MachineAllowance = data.MachineAllowance;
        //$scope.operationVariationNew.AdditionalAllowance = data.AdditionalAllowance;

        $scope.machine.MachineVarientId = $scope.operationVariationNew.ArticleId;
        // $scope.machine.SkillId = $scope.operationVariationNew.SkillId;

        // getOperationVariationUtilityData($scope.operationId, $scope.operationVariationNew.ArticleId, $scope.operationVariationNew.SkillId);

        $scope.closeArticle();
        $scope.closeMaterial();
    };

    $scope.closeArticle = function () {
        angular.element(document.querySelector('#articleId')).modal('hide');
    };

    // #endregion Article

    // #region Skill

    $scope.skillList = [];
    $scope.skillParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'UserName'
        , searchBy: "UserName"
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };
    $scope.skillPoUp = function () {
        var opProcessIds = $.grep($scope.operationList, function (item) { return item.Value === $scope.operationVariationNew.OperationId; })[0].ProsessIds;
        var opProcessArray = opProcessIds.split(',');
        $scope.excluedList = [];
        $scope.skillDataList = [];
        $scope.skillUrl = 'Skills/Skill/GetCommonSkillListByProcess?processIds=' + JSON.stringify(opProcessArray);
        baseService.setCurrentPage('dataList');
        $scope.getSkillData = function (pageno) {
            baseService.paginationBase($scope.skillUrl, pageno, $scope.skillParameters)
                .then(function (result) {
                    $scope.skillDataList = result.Rows;
                    $scope.skillParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.skillList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.skillList);
                    }
                    angular.element(document.querySelector('#skillId')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'skillId');
                }).finally(function () {
                });
        };
        $scope.getSkillData();
    };
    $scope.selectSkill = function (data) {
        $scope.operationVariationNew.ArticleId = null;
        $scope.operationVariationNew.ArticleName = null;
        $scope.operationVariationNew.SkillId = data.SkillId;
        $scope.operationVariationNew.SkillName = data.UserName;
        $scope.operationVariationNew.MachineAllowance = 0;
        $scope.closeSkill();
    };
    $scope.closeSkill = function () {
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#skillId')).modal('hide');
    };

    // #endregion Skill

    $scope.operationVariationNew = {
        ArticleId: null
        , ArticleName: null
        , SkillId: null
        , SkillName: null
        , BasicProcessTime: 0
        , AssociateProcessTime: 0
        , PersonalAllowance: 0
        , MachineAllowance: 0
        , Frequency: 0
        , SPI: 0
        , IsMachineRequired: false
        , AdditionalSAMSymbol: '+'
        , AdditionalAllowance: 0
    }

    function getOperationVariationUtilityData(operationId, articleId, skillId) {
        $http.get('machines/OperationVariation/GetUtilityByOperationData?operationId=' + operationId + '&articleId=' + articleId + '&skillId=' + skillId)
            .then(function (response) {
                // console.log(response.data);
                if (!baseService.isUndefinedOrNull(response.data.ArticleId)) {
                    $scope.operationVariationNew.ArticleId = response.data.ArticleId;
                    $scope.operationVariationNew.ArticleName = response.data.ArticleName;
                    $scope.operationVariationNew.MaterialName = response.data.MaterialName;

                    $scope.operationVariationNew.SkillId = response.data.SkillId;
                    $scope.operationVariationNew.SkillName = response.data.SkillName;

                    $scope.operationVariationNew.BasicProcessTime = response.data.BasicProcessTime;
                    $scope.operationVariationNew.AssociateProcessTime = response.data.AssociateProcessTime;
                    $scope.operationVariationNew.PersonalAllowance = response.data.PersonalAllowance;
                    $scope.operationVariationNew.MachineAllowance = response.data.MachineAllowance;
                    $scope.operationVariationNew.AdditionalAllowance = response.data.AdditionalAllowance;

                    $scope.operationVariationNew.Frequency = response.data.Frequency;
                    $scope.operationVariationNew.SPI = response.data.SPI;
                    $scope.operationVariationNew.IsMachineRequired = response.data.IsMachineRequired;
                    $scope.operationVariationNew.TotalSAM = response.data.TotalSAM;
                    $scope.operationVariationNew.AdditionalSAMSymbol = response.data.AdditionalSAMSymbol;
                    $scope.operationVariationNew.AdditionalSAM = response.data.AdditionalSAM;
                    $scope.operationVariationNew.SubOperationSAM = response.data.SubOperationSAM;
                    $scope.data.OperationSPT = $scope.operationVariationNew.TotalSAM;


                    var firstSam = parseFloat($scope.operationVariationNew.BasicProcessTime) + parseFloat($scope.operationVariationNew.AssociateProcessTime);
                    var sam = (firstSam * $scope.operationVariationNew.PersonalAllowance / 100
                        + firstSam * $scope.operationVariationNew.MachineAllowance / 100) + firstSam;
                    $scope.operationVariationNew.SAM = sam;
                    $scope.operationVariationNew.SubOperationSAM = sam.toFixed(2);
                    $scope.data.OperationSPT = sam.toFixed(2);

                    if (!baseService.isUndefinedOrNull($scope.operationVariationNew.SubOperationSAM)) {
                        var total = eval(parseFloat($scope.operationVariationNew.SubOperationSAM) + $scope.operationVariationNew.AdditionalSAMSymbol + "(" + $scope.operationVariationNew.AdditionalSAM + ")");
                        $scope.operationVariationNew.TotalSAM = total.toFixed(2);

                        for (var i = 0; i < $scope.operationList.length; i++) {
                            if ($scope.operationList[i].OperationVariationId === $scope.data.OperationVariationId) {
                                $scope.operationList[i].OperationSPT = parseFloat($scope.data.OperationSPT);
                                $scope.operationList[i].MachineVarientId = $scope.operationVariationNew.ArticleId;
                                $scope.operationList[i].TotalSPT = $scope.operationVariationNew.TotalSAM;
                            }
                        }

                        var gridObj = $("#GridBulOperation").data("ejGrid");
                        gridObj.refreshContent(true);
                    }

                } else {
                    ShowResult('No data found in Operation Variation.', 'failure', 'MachinePopUp');
                }

            });
    }

    function calculateSAM() {
        var firstSam = parseFloat($scope.operationVariationNew.BasicProcessTime) + parseFloat($scope.operationVariationNew.AssociateProcessTime);
        var sam = (firstSam * $scope.operationVariationNew.PersonalAllowance / 100
            + firstSam * $scope.operationVariationNew.MachineAllowance / 100) + firstSam;
        $scope.operationVariationNew.SAM = sam;
        $scope.operationVariationNew.SubOperationSAM = sam.toFixed(2);
        $scope.data.OperationSPT = sam.toFixed(2);

        if (!baseService.isUndefinedOrNull($scope.operationVariationNew.SubOperationSAM)) {
            var total = eval(parseFloat($scope.operationVariationNew.SubOperationSAM) + $scope.operationVariationNew.AdditionalSAMSymbol + "(" + $scope.operationVariationNew.AdditionalSAM + ")");
            $scope.operationVariationNew.TotalSAM = total.toFixed(2);

            for (var i = 0; i < $scope.operationList.length; i++) {
                if ($scope.operationList[i].OperationVariationId === $scope.data.OperationVariationId) {
                    $scope.operationList[i].OperationSPT = parseFloat($scope.data.OperationSPT);
                    $scope.operationList[i].MachineVarientId = $scope.operationVariationNew.ArticleId;
                    $scope.operationList[i].TotalSPT = $scope.operationVariationNew.TotalSAM;
                }
            }

            var gridObj = $("#GridBulOperation").data("ejGrid");
            gridObj.refreshContent(true);
        }

    }

    $scope.MCtotalspt = 0;
    $scope.NonMCtotalspt = 0;

    $scope.TotalMP = 0;
    $scope.MCtotalMP = 0;
    $scope.NonMCtotalMP = 0;

    $scope.CalculateGroup = function () {
        $scope.TotalSPT = 0;
        $scope.TotalWorkStation = 0;
        $scope.TotalManpower = 0;
        $scope.OrganizationEfficiency = 0;
        $scope.ProductionEfficiencyPerHour = 0;
        $scope.TotalManpower = 0;
        $scope.PitchTime = 0;
        $scope.ProductionEfficiencyPerDay = 0;
        $scope.MaxAllottedTime = 0;
        $scope.LineTargetPerHour = 0;
        $scope.MCtotalspt = 0;
        $scope.NonMCtotalspt = 0;

        var MaxNumber = 0;
        var MaxNumberIndex = -1;
        var totalspt = 0;
        var totalMP = 0;
        var MCtotalspt = 0;
        var NonMCtotalspt = 0;
        var aatarray = [];
        var TotalWoS = 0;

        var TotalMP = 0;
        var MCtotalMP = 0;
        var NonMCtotalMP = 0;

        if (baseService.arrayLength($scope.operationList) > 0) {
            for (var i = 0; i < $scope.operationList.length; i++) {
                if (!baseService.isUndefinedOrNull($scope.operationList[i].OperationGroup)) {
                    $scope.SPTSum = $filter("sumByKey")($filter("filter")($scope.operationList, { "OperationGroup": $scope.operationList[i].OperationGroup }), "TotalSPT");
                    $scope.AMSum = $filter("sumByKey")($filter("filter")($scope.operationList, { "OperationGroup": $scope.operationList[i].OperationGroup }), "AllotedManpower");
                    $scope.operationList[i].AvgAllotedTime = ($scope.SPTSum / $scope.AMSum).toFixed(2);
                } else {
                    if ($scope.operationList[i].AllotedManpower !== 0) {
                        $scope.operationList[i].AvgAllotedTime = ($scope.operationList[i].TotalSPT / $scope.operationList[i].AllotedManpower).toFixed(2);
                    } else {
                        $scope.operationList[i].AvgAllotedTime = 0;
                    }
                }
                $scope.operationList[i].OperationTargetPerHr = Math.round(60 / $scope.operationList[i].TotalSPT);
                $scope.operationList[i].RequiredManPower = ($scope.RequiredStdTarget / (60 / $scope.operationList[i].TotalSPT)).toFixed(2);

                $scope.operationList[i].IsMaxAllottedTime = false;
                if (parseFloat($scope.operationList[i].AvgAllotedTime) > MaxNumber) {
                    MaxNumber = parseFloat($scope.operationList[i].AvgAllotedTime);
                    MaxNumberIndex = i;
                }

                totalspt = totalspt + $scope.operationList[i].TotalSPT;
                totalMP = totalMP + $scope.operationList[i].AllotedManpower;
                TotalWoS = TotalWoS + $scope.operationList[i].AllotedWorkstation;

                if (!baseService.isUndefinedOrNull($scope.operationList[i].MachineVarientId)) {
                    MCtotalspt = MCtotalspt + $scope.operationList[i].TotalSPT;
                    MCtotalMP = MCtotalMP + $scope.operationList[i].AllotedManpower;
                }

                if (baseService.isUndefinedOrNull($scope.operationList[i].MachineVarientId)) {
                    NonMCtotalspt = NonMCtotalspt + $scope.operationList[i].TotalSPT;
                    NonMCtotalMP = NonMCtotalMP + $scope.operationList[i].AllotedManpower;
                }


                aatarray.push(parseFloat($scope.operationList[i].AvgAllotedTime));

            }

            //$scope.operationList[MaxNumberIndex].IsMaxAllottedTime = true;

            var pitchTime = (totalspt / totalMP).toFixed(2);
            var avgat = Math.max.apply(null, aatarray);

            for (var i = 0; i < $scope.operationList.length; i++) {
                if (parseFloat($scope.operationList[i].AvgAllotedTime) == avgat) {
                    $scope.operationList[i].IsMaxAllottedTime = true;
                }
            }

            var ob = {};
            ob.PitchTime = pitchTime;
            ob.MaxAllottedTime = avgat;
            ob.OrganizationEfficiency = (ob.PitchTime / ob.MaxAllottedTime).toFixed(2);
            ob.ProductionEfficiencyPerHour = ((totalMP * 60) / totalspt).toFixed(2);
            ob.ProductionEfficiencyPerDay = (ob.ProductionEfficiencyPerHour * $scope.PlannedHoursPerDay).toFixed(2);
            ob.LineTargetPerHour = (ob.ProductionEfficiencyPerHour * ob.OrganizationEfficiency).toFixed(2);

            // ((totalMP * 60) / totalspt) * ((totalspt / totalMP) / ob.MaxAllottedTime)

            $scope.PitchTime = pitchTime;
            $scope.MaxAllottedTime = avgat;
            $scope.OrganizationEfficiency = (ob.PitchTime / ob.MaxAllottedTime).toFixed(2);
            $scope.ProductionEfficiencyPerHour = Math.round(((totalMP * 60) / totalspt).toFixed(2));
            $scope.ProductionEfficiencyPerDay = Math.round((ob.ProductionEfficiencyPerHour * $scope.PlannedHoursPerDay).toFixed(2));
            $scope.LineTargetPerHour = Math.round((ob.ProductionEfficiencyPerHour * ob.OrganizationEfficiency).toFixed(2));

            $scope.TotalSPT = totalspt.toFixed(2);
            $scope.TotalManpower = totalMP.toFixed(2);
            $scope.TotalWorkStation = TotalWoS;

            $scope.MCtotalspt = MCtotalspt.toFixed(2);
            $scope.NonMCtotalspt = NonMCtotalspt.toFixed(2);

            $scope.MCtotalMP = MCtotalMP.toFixed(2);
            $scope.NonMCtotalMP = NonMCtotalMP.toFixed(2);

        }

        var gridObj = $("#GridBulOperation").data("ejGrid");
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();
    };

    $window.onresize = function (event) {
        $scope.actionComplete();
        $scope.actionMacComplete();
    };

    $scope.actionComplete = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridBulOperation").ejGrid("instance");
                var scrollerwidth = $("#processbuyer").width();//Obtain the width of the container
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 300, width: 1080 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            // $scope.ShowResultCustom(e, 'failure');
        }
    };

    $scope.actionMacComplete = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridBulMacOperation").ejGrid("instance");
                var scrollerwidth = $("#processbuyer").width();//Obtain the width of the container
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 300, width: 1080 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            // $scope.ShowResultCustom(e, 'failure');
        }
    };


    $scope.TotalSPT = 0;
    $scope.TotalManpower = 0;
    $scope.TotalWorkStation = 0;

    function CheckSequence() {
        var arr = [];
        for (var i = 0; i < $scope.operationList.length; i++) {
            if (!baseService.isUndefinedOrNull($scope.operationList[i].Sequence)) {
                if (checkExistsSS(arr, $scope.operationList[i].Sequence) === false) {
                    arr.push($scope.operationList[i].Sequence);
                }
                else {
                    throw "Sequence " + $scope.operationList[i].Sequence + " is exists for " + $scope.operationList[i].OperationVariation + ".";
                }
            }
        }
        $scope.CalculateGroup();
    }

    function checkExistsSS(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i] === id) {
                return true;
            }
        }
        return false;
    }

    $scope.calculatedBulletinModel = { Id: Math.floor(Math.random() * 9) - 10, TotalSPT: 0, TotalManpower: 0, TotalWorkStation: 0, MCtotalspt: 0, MCtotalMP: 0, NonMCtotalspt: 0, NonMCtotalMP: 0, PitchTime: 0, ProductionEfficiencyPerHour: 0, MaxAllottedTime: 0, ProductionEfficiencyPerDay: 0, OrganizationEfficiency: 0, LineTargetPerHour:0};

    $scope.SaveOperation = function () {
        try {
            if (baseService.arrayLength($scope.operationList) < 0) {
                throw "Select Opearation.";
            }
            CheckSequence();
            $scope.calculatedBulletinModel.TotalSPT = $scope.TotalSPT;
            $scope.calculatedBulletinModel.TotalManpower = $scope.TotalManpower;
            $scope.calculatedBulletinModel.TotalWorkStation = $scope.TotalWorkStation;
            $scope.calculatedBulletinModel.MCtotalspt = $scope.MCtotalspt;
            $scope.calculatedBulletinModel.MCtotalMP = $scope.MCtotalMP;
            $scope.calculatedBulletinModel.NonMCtotalspt = $scope.NonMCtotalspt;
            $scope.calculatedBulletinModel.NonMCtotalMP = $scope.NonMCtotalMP;
            $scope.calculatedBulletinModel.PitchTime = $scope.PitchTime;
            $scope.calculatedBulletinModel.ProductionEfficiencyPerHour = $scope.ProductionEfficiencyPerHour;
            $scope.calculatedBulletinModel.MaxAllottedTime = $scope.MaxAllottedTime;
            $scope.calculatedBulletinModel.ProductionEfficiencyPerDay = $scope.ProductionEfficiencyPerDay;
            $scope.calculatedBulletinModel.OrganizationEfficiency = $scope.OrganizationEfficiency;
            $scope.calculatedBulletinModel.LineTargetPerHour = $scope.LineTargetPerHour;
            $scope.calculatedBulletinModel.BulletinTemplateMasterId = $scope.BulletinTemplateMasterId;

            for (var i = 0; i < $scope.operationList.length; i++) {
                $scope.operationList[i].BulletinTemplateMasterId = $scope.BulletinTemplateMasterId;
            }
            $http({
                method: 'POST',
                url: $scope.saveOperationUrl,
                data: { 'bulletinTemplateDetails': $scope.operationList, 'bulletinTemplateMasterId': $scope.BulletinTemplateMasterId, 'calculatedBulletinModel': $scope.calculatedBulletinModel},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //$scope.operationList = [];
                    $scope.getSavedOperationData($scope.BulletinTemplateMasterId);
                    //  $scope.GetProcessCountData();
                    //$scope.GetProcessPitchCountData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.machine = {
        Id: null,
        MachineVarientId: null,
        SkillId: null
    }

    $scope.UpdateMachine = function () {
        try {

            $scope.machine.Id = $scope.BulletinTemplateDetailId;
            $scope.machine.MachineVarientId = $scope.operationVariationNew.ArticleId;
            $scope.machine.SkillId = $scope.operationVariationNew.SkillId;

            $http({
                method: 'POST',
                url: $scope.saveMachineUrl,
                data: $scope.machine,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //$scope.operationList = [];
                    $scope.getSavedOperationData($scope.BulletinTemplateMasterId);
                    //$scope.GetProcessCountData();
                    $scope.machine = {};
                    angular.element(document.querySelector('#MachinePopUp')).modal('hide');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.removeProcessOperation = function (obj) {
        var gridObj = $("#GridBulOperation").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.bulletinTemplateDetail = data;
        if (!baseService.isUndefinedOrNull($scope.bulletinTemplateDetail.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently [' + $scope.bulletinTemplateDetail.OperationVariation + ' ]';
        angular.element(document.querySelector('#confirmProcessOperationPopUp')).modal('show');
    }

    $scope.DeleteProcessOperation = function () {
        $http({
            method: 'POST',
            url: 'IE/BulletinTemplate/DeleteOperation?id=' + $scope.bulletinTemplateDetail.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                //$scope.operationList = [];
                $scope.getSavedOperationData($scope.BulletinTemplateMasterId);

            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    };

    $scope.GetSequence = function () {
        var gridObj = $("#GridBulOperation").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.bulletinTemplateDetailNew = Object.assign({}, data);
        angular.element(document.querySelector('#SeqPopup')).modal('show');
    }
    $scope.closeSeqPopUp = function () {
        angular.element(document.querySelector('#SeqPopup')).modal('hide');
    }

    $scope.UpdateSequence = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.saveSeqUrl,
                data: $scope.bulletinTemplateDetailNew,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //$scope.operationList = [];
                    $scope.getSavedOperationData($scope.BulletinTemplateMasterId);
                    //$scope.GetProcessCountData();
                    angular.element(document.querySelector('#SeqPopup')).modal('hide');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.GetOperationVaiationCode = function () {
        var gridObj = $("#GridBulOperation").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.bulletinTemplateDetailNew = Object.assign({}, data);
        angular.element(document.querySelector('#OperationVaiationCodePopup')).modal('show');
    }

    $scope.closeOperationVaiationCodePopUp = function () {
        angular.element(document.querySelector('#OperationVaiationCodePopup')).modal('hide');
    }

    $scope.UpdateOperationVaiationCode = function () {
        try {

            if (checkCodeExists($scope.operationList, $scope.bulletinTemplateDetailNew.OperationVaiationCode) === false) {
                $http({
                    method: 'POST',
                    url: $scope.UpdateOperationVaiationCodeUrl,
                    data: { 'bulletinTemplateDetail': $scope.bulletinTemplateDetailNew, 'processId': $scope.ProcessId, 'bulletinTemplateMasterId': $scope.BulletinTemplateMasterId },

                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        //$scope.operationList = [];
                        $scope.getSavedOperationData($scope.BulletinTemplateMasterId);
                        //$scope.GetProcessCountData();
                        angular.element(document.querySelector('#OperationVaiationCodePopup')).modal('hide');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            } else {
                throw "This Operation Code " + $scope.bulletinTemplateDetailNew.OperationCode + " is already taken.";
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    // #endregion Operation       

    //#region Operatoin Thread Consumption
    $scope.businessProcesses = "ThreadConsumption";
    $scope.materialType = null;

    // #region Needle Material Article Search By Business Process

    $scope.materialmasterSearchData = [];
    //$scope.searchList = [];
    //$scope.dataPlate = [];
    $scope.searchbyMaterialMasterDatalist = [
        {
            'name': 'Material Type',
            'value': 'MaterialTypeName'
        },
        {
            'name': 'Material Group',
            'value': 'MaterialGroupMasterName'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Product',
            'value': 'ProductMasterName'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'IsAsset',
            'value': 'IsAsset'
        },
        {
            'name': 'Asset Master',
            'value': 'AssetMasterName'
        },
        {
            'name': 'Budget Code',
            'value': 'AssetBudgetCode'
        },
        {
            'name': 'Activity',
            'value': 'ActivityName'
        },
        {
            'name': 'Id',
            'value': 'Id'
        }
    ];

    $scope.getNeedleMaterialMasterSearchData = function (args) {
        var gridObj = $("#GridBulMacOperation").data("ejGrid");
        $scope.bulletinTemplateDetailNew = gridObj.getSelectedRecords()[0];
        $scope.mmPopUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'UserName'
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.materialTitle = 'Material';
        CloseShowResult();
        CloseModalShowResult();
        $scope.searchList = [];
        $scope.materialmasterSearchData = [];
        $scope.popUpUrl = 'Materials/MaterialMaster/MaterialSearchByBusinessProcess?type=' + $scope.businessProcesses;
        baseService.setCurrentPage('materialmasterSearchData');
        $scope.loadMMData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.mmPopUpParameters)
                .then(function (result) {
                    $scope.materialmasterSearchData = result.Rows;
                    $scope.mmPopUpParameters.total_count = result.Total;
                    angular.element(document.querySelector('#NeedlematerialMasterbyTypePopup')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        $scope.loadMMData();
    };

    $scope.setNeedleMaterialMasterData = function (ob) {
        $scope.bulletinTemplateDetailNew.NeedleMaterialMasterId = ob.Id;
        $scope.bulletinTemplateDetailNew.NeedleMaterialMaster = ob.UserName;
        $scope.bulletinTemplateDetailNew.NeedleArticleId = null;
        $scope.bulletinTemplateDetailNew.NeedleArticle = null;
        $scope.bulletinTemplateDetailNew.HasAttribute = ob.HasAttribute;

        if ($scope.bulletinTemplateDetailNew.HasAttribute) {
            $scope.materialType = null;
            $scope.getNeedleArticleSearchList(ob.Id);
        } else {
            $scope.closeNeedleMaterialMasterbyTypePopUp();
            return ShowResult('This material has no attribute', 'failure');
        }

        $scope.closeNeedleMaterialMasterbyTypePopUp();

    };

    $scope.closeNeedleMaterialMasterbyTypePopUp = function () {
        CloseModalShowResult('NeedlematerialMasterbyTypePopup');
        angular.element(document.querySelector('#NeedlematerialMasterbyTypePopup')).modal('hide');

    };

    $scope.ClearNeedleMaterialMasterSearchData = function (ob) {
        var gridObj = $("#GridBulMacOperation").data("ejGrid");
        $scope.bulletinTemplateDetailNew = gridObj.getSelectedRecords()[0];
        $scope.bulletinTemplateDetailNew.NeedleMaterialMasterId = null;
        $scope.bulletinTemplateDetailNew.NeedleMaterialMaster = null;
        $scope.bulletinTemplateDetailNew.NeedleArticleId = null;
        $scope.bulletinTemplateDetailNew.NeedleArticle = null;

        gridObj.refreshContent(true);
        gridObj.refreshTemplate();
    };

    $scope.getNeedleArticleSearchList = function (id) {
        try {
            CloseShowResult();
            CloseModalShowResult();
            $scope.articlePopUpParameters = {
                limit: 10
                , offset: 0
                , order: 'asc'
                , sort: 'StandardName'
                , searchBy: "StandardName"
                , pageSize: 10
                , total_count: 0
                , search: null
                , serverPagination: true
            };
            $scope.searchList = [];
            $scope.dataPlate = [];
            baseService.setCurrentPage('dataPlate');
            $scope.articlePopUpParameters.materialMasterId = id;
            $scope.articlePopUpParameters.materialType = JSON.stringify($scope.materialType);
            $scope.loadArticleData = function (pageno) {
                baseService.paginationBase('Materials/MaterialMasterArticle/GetMaterialArticle', pageno, $scope.articlePopUpParameters)
                    .then(function (result) {
                        //if (baseService.arrayLength(result.Rows) === 0) return ShowResult('This material has no article', 'failure');
                        $scope.dataPlate = result.Rows;
                        $scope.articlePopUpParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.searchList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchList);
                        if ($scope.articlePopUpParameters.total_count == 0) {
                            ShowResult("This material has no article ", 'failure');
                        }
                        else {
                            angular.element(document.querySelector('#NeedlearticleSearchPop')).modal('show');
                        }

                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            $scope.loadArticleData();
        } catch (e) {
            ShowResult(e, '');
        }
    };
    $scope.closeMaterialArticlePopUp = function () {
        $scope.searchList = [];
        $scope.dataPlate = [];
        $scope.popUpUrl = '';
        CloseModalShowResult('NeedlearticleSearchPop');
        angular.element(document.querySelector('#NeedlearticleSearchPop')).modal('hide');
    };

    $scope.selectNeedlearticle = function (ob) {
        try {
            $scope.bulletinTemplateDetailNew.NeedleMaterialMasterId = ob.MaterialMasterId;
            $scope.bulletinTemplateDetailNew.NeedleMaterialMaster = ob.MaterialMasterName;
            $scope.bulletinTemplateDetailNew.NeedleArticleId = ob.Id;
            $scope.bulletinTemplateDetailNew.NeedleArticle = ob.StandardName;
            angular.element(document.querySelector('#NeedlearticleSearchPop')).modal('hide');

            var gridObj = $("#GridBulMacOperation").data("ejGrid");
            gridObj.refreshContent();

        } catch (e) {
            ShowResult(e, '', 'NeedlearticleSearchPop');
        }
    };

    // #endregion Needle Material Article Search

    // #region Bobbin Material Article Search By Business Process
    $scope.materialmasterSearchData = [];
    //$scope.searchList = [];
    //$scope.dataPlate = [];

    $scope.getBobbinMaterialMasterSearchData = function (args) {
        var gridObj = $("#GridBulMacOperation").data("ejGrid");
        $scope.bulletinTemplateDetailNew = gridObj.getSelectedRecords()[0];
        $scope.mmPopUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'UserName'
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.materialTitle = 'Material';
        CloseShowResult();
        CloseModalShowResult();
        $scope.searchList = [];
        $scope.materialmasterSearchData = [];
        $scope.popUpUrl = 'Materials/MaterialMaster/MaterialSearchByBusinessProcess?type=' + $scope.businessProcesses;
        baseService.setCurrentPage('materialmasterSearchData');
        $scope.loadMMData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.mmPopUpParameters)
                .then(function (result) {
                    $scope.materialmasterSearchData = result.Rows;
                    $scope.mmPopUpParameters.total_count = result.Total;
                    angular.element(document.querySelector('#BobbinmaterialMasterbyTypePopup')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        $scope.loadMMData();
    };

    $scope.setBobbinMaterialMasterData = function (ob) {
        $scope.bulletinTemplateDetailNew.BobbinMaterialMasterId = ob.Id;
        $scope.bulletinTemplateDetailNew.BobbinMaterialMaster = ob.UserName;
        $scope.bulletinTemplateDetailNew.BobbinArticleId = null;
        $scope.bulletinTemplateDetailNew.BobbinArticle = null;
        $scope.bulletinTemplateDetailNew.HasAttribute = ob.HasAttribute;

        if ($scope.bulletinTemplateDetailNew.HasAttribute) {
            $scope.materialType = null;
            $scope.getBobbinArticleSearchList(ob.Id);
        } else {
            $scope.closeBobbinMaterialMasterbyTypePopUp();
            return ShowResult('This material has no attribute', 'failure');
        }

        $scope.closeBobbinMaterialMasterbyTypePopUp();

    };

    $scope.closeBobbinMaterialMasterbyTypePopUp = function () {
        CloseModalShowResult('BobbinmaterialMasterbyTypePopup');
        angular.element(document.querySelector('#BobbinmaterialMasterbyTypePopup')).modal('hide');

    };

    $scope.getBobbinArticleSearchList = function (id) {
        try {
            CloseShowResult();
            CloseModalShowResult();
            $scope.articlePopUpParameters = {
                limit: 10
                , offset: 0
                , order: 'asc'
                , sort: 'StandardName'
                , searchBy: "StandardName"
                , pageSize: 10
                , total_count: 0
                , search: null
                , serverPagination: true
            };
            $scope.searchList = [];
            $scope.dataPlate = [];
            baseService.setCurrentPage('dataPlate');
            $scope.articlePopUpParameters.materialMasterId = id;
            $scope.articlePopUpParameters.materialType = JSON.stringify($scope.materialType);
            $scope.loadArticleData = function (pageno) {
                baseService.paginationBase('Materials/MaterialMasterArticle/GetMaterialArticle', pageno, $scope.articlePopUpParameters)
                    .then(function (result) {
                        //if (baseService.arrayLength(result.Rows) === 0) return ShowResult('This material has no article', 'failure');
                        $scope.dataPlate = result.Rows;
                        $scope.articlePopUpParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.searchList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchList);
                        if ($scope.articlePopUpParameters.total_count == 0) {
                            ShowResult("This material has no article ", 'failure');
                        }
                        else {
                            angular.element(document.querySelector('#BobbinarticleSearchPop')).modal('show');
                        }

                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            $scope.loadArticleData();
        } catch (e) {
            ShowResult(e, '');
        }
    };
    $scope.closeMaterialArticlePopUp = function () {
        $scope.searchList = [];
        $scope.dataPlate = [];
        $scope.popUpUrl = '';
        CloseModalShowResult('BobbinarticleSearchPop');
        angular.element(document.querySelector('#BobbinarticleSearchPop')).modal('hide');
    };

    $scope.selectBobbinarticle = function (ob) {
        try {
            $scope.bulletinTemplateDetailNew.BobbinMaterialMasterId = ob.MaterialMasterId;
            $scope.bulletinTemplateDetailNew.BobbinMaterialMaster = ob.MaterialMasterName;
            $scope.bulletinTemplateDetailNew.BobbinArticleId = ob.Id;
            $scope.bulletinTemplateDetailNew.BobbinArticle = ob.StandardName;
            angular.element(document.querySelector('#BobbinarticleSearchPop')).modal('hide');

            var gridObj = $("#GridBulMacOperation").data("ejGrid");
            gridObj.refreshContent();

        } catch (e) {
            ShowResult(e, '', 'BobbinarticleSearchPop');
        }
    };


    $scope.ClearBobbinMaterialMasterSearchData = function (ob) {
        var gridObj = $("#GridBulMacOperation").data("ejGrid");
        $scope.bulletinTemplateDetailNew = gridObj.getSelectedRecords()[0];
        $scope.bulletinTemplateDetailNew.BobbinMaterialMasterId = null;
        $scope.bulletinTemplateDetailNew.BobbinMaterialMaster = null;
        $scope.bulletinTemplateDetailNew.BobbinArticleId = null;
        $scope.bulletinTemplateDetailNew.BobbinArticle = null;
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();
    };

    // #endregion Bobbin Material Article Search

    // #region Looper Material Article Search By Business Process
    $scope.materialmasterSearchData = [];
    //$scope.searchList = [];
    //$scope.dataPlate = [];

    $scope.getLooperMaterialMasterSearchData = function (args) {
        var gridObj = $("#GridBulMacOperation").data("ejGrid");
        $scope.bulletinTemplateDetailNew = gridObj.getSelectedRecords()[0];
        $scope.mmPopUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'UserName'
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.materialTitle = 'Material';
        CloseShowResult();
        CloseModalShowResult();
        $scope.searchList = [];
        $scope.materialmasterSearchData = [];
        $scope.popUpUrl = 'Materials/MaterialMaster/MaterialSearchByBusinessProcess?type=' + $scope.businessProcesses;
        baseService.setCurrentPage('materialmasterSearchData');
        $scope.loadMMData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.mmPopUpParameters)
                .then(function (result) {
                    $scope.materialmasterSearchData = result.Rows;
                    $scope.mmPopUpParameters.total_count = result.Total;
                    angular.element(document.querySelector('#LoopermaterialMasterbyTypePopup')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        $scope.loadMMData();
    };

    $scope.setLooperMaterialMasterData = function (ob) {
        $scope.bulletinTemplateDetailNew.LooperMaterialMasterId = ob.Id;
        $scope.bulletinTemplateDetailNew.LooperMaterialMaster = ob.UserName;
        $scope.bulletinTemplateDetailNew.LooperArticleId = null;
        $scope.bulletinTemplateDetailNew.LooperArticle = null;
        $scope.bulletinTemplateDetailNew.HasAttribute = ob.HasAttribute;

        if ($scope.bulletinTemplateDetailNew.HasAttribute) {
            $scope.materialType = null;
            $scope.getLooperArticleSearchList(ob.Id);
        } else {
            $scope.closeLooperMaterialMasterbyTypePopUp();
            return ShowResult('This material has no attribute', 'failure');
        }

        $scope.closeLooperMaterialMasterbyTypePopUp();

    };

    $scope.closeLooperMaterialMasterbyTypePopUp = function () {
        CloseModalShowResult('LoopermaterialMasterbyTypePopup');
        angular.element(document.querySelector('#LoopermaterialMasterbyTypePopup')).modal('hide');

    };

    $scope.getLooperArticleSearchList = function (id) {
        try {
            CloseShowResult();
            CloseModalShowResult();
            $scope.articlePopUpParameters = {
                limit: 10
                , offset: 0
                , order: 'asc'
                , sort: 'StandardName'
                , searchBy: "StandardName"
                , pageSize: 10
                , total_count: 0
                , search: null
                , serverPagination: true
            };
            $scope.searchList = [];
            $scope.dataPlate = [];
            baseService.setCurrentPage('dataPlate');
            $scope.articlePopUpParameters.materialMasterId = id;
            $scope.articlePopUpParameters.materialType = JSON.stringify($scope.materialType);
            $scope.loadArticleData = function (pageno) {
                baseService.paginationBase('Materials/MaterialMasterArticle/GetMaterialArticle', pageno, $scope.articlePopUpParameters)
                    .then(function (result) {
                        //if (baseService.arrayLength(result.Rows) === 0) return ShowResult('This material has no article', 'failure');
                        $scope.dataPlate = result.Rows;
                        $scope.articlePopUpParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.searchList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchList);
                        if ($scope.articlePopUpParameters.total_count == 0) {
                            ShowResult("This material has no article ", 'failure');
                        }
                        else {
                            angular.element(document.querySelector('#LooperarticleSearchPop')).modal('show');
                        }

                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            $scope.loadArticleData();
        } catch (e) {
            ShowResult(e, '');
        }
    };
    $scope.closeMaterialArticlePopUp = function () {
        $scope.searchList = [];
        $scope.dataPlate = [];
        $scope.popUpUrl = '';
        CloseModalShowResult('LooperarticleSearchPop');
        angular.element(document.querySelector('#LooperarticleSearchPop')).modal('hide');
    };

    $scope.selectLooperarticle = function (ob) {
        try {
            $scope.bulletinTemplateDetailNew.LooperMaterialMasterId = ob.MaterialMasterId;
            $scope.bulletinTemplateDetailNew.LooperMaterialMaster = ob.MaterialMasterName;
            $scope.bulletinTemplateDetailNew.LooperArticleId = ob.Id;
            $scope.bulletinTemplateDetailNew.LooperArticle = ob.StandardName;
            angular.element(document.querySelector('#LooperarticleSearchPop')).modal('hide');

            var gridObj = $("#GridBulMacOperation").data("ejGrid");
            gridObj.refreshContent();

        } catch (e) {
            ShowResult(e, '', 'LooperarticleSearchPop');
        }
    };

    $scope.ClearLooperMaterialMasterSearchData = function (ob) {
        var gridObj = $("#GridBulMacOperation").data("ejGrid");
        $scope.bulletinTemplateDetailNew = gridObj.getSelectedRecords()[0];
        $scope.bulletinTemplateDetailNew.LooperMaterialMasterId = null;
        $scope.bulletinTemplateDetailNew.LooperMaterialMaster = null;
        $scope.bulletinTemplateDetailNew.LooperArticleId = null;
        $scope.bulletinTemplateDetailNew.LooperArticle = null;
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();
    };

    // #endregion Looper Material Article Search

    $scope.WastagePercentage = null;
    $scope.ExtraOrderPercentage = null;

    $scope.SaveThreadOperation = function () {
        try {
            if (baseService.arrayLength($scope.machineOperationList) < 0) {
                throw "Select Opearation.";
            }

            for (var i = 0; i < $scope.machineOperationList.length; i++) {
                $scope.machineOperationList[i].BulletinTemplateMasterId = $scope.BulletinTemplateMasterId;
                $scope.machineOperationList[i].WastagePercentage = $scope.WastagePercentage;
                $scope.machineOperationList[i].ExtraOrderPercentage = $scope.ExtraOrderPercentage;
            }
            $http({
                method: 'POST',
                url: $scope.saveOperationUrl,
                data: { 'bulletinTemplateDetails': $scope.machineOperationList, 'bulletinTemplateMasterId': $scope.BulletinTemplateMasterId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSavedOperationData($scope.BulletinTemplateMasterId);
                    $scope.getSavedMacnineOperationData($scope.BulletinTemplateMasterId);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };



    //#endregion Operatoin Thread Consumption

    //#region start BulletinTamplate Reports

    $scope.GetBulletinTamplateIndexReport = function () {
        var reportFormat = "Excel";
        try {
            var url = 'IE/bulletintemplate/GetBulletinTamplateIndexReport?reportFormat=' + reportFormat;

            $rootScope.report(url);
        } catch (e) {

        }
    };


    //$scope.onClickExcelPrint = function (args) {

    //    var data = args.data;
    //    var reportFormat = "Excel";

    //    try {
    //        window.open('IE/bulletintemplate/GetBulletinTamplateDetailReport?reportFormat=' + reportFormat + '&bulletinTemplateId=' + data.Id, '_blank');

    //    } catch (e) {

    //    }
    //};

    $scope.onClickExcelPrints = function (args) {

        try {
            var data = args.data;
            var reportFormat = "Excel";

            var file_src = 'IE/bulletintemplate/GetBulletinTamplateDetailReport?reportFormat=' + reportFormat + '&bulletinTemplateId=' + data.Id
            $rootScope.report(file_src);

        } catch (e) {

        }
    }
    $scope.onClickExcelPrintspdf = function (args) {

        try {
            var data = args.data;
            var reportFormat = "Pdf";

            var file_src = 'IE/bulletintemplate/GetBulletinTamplateDetailReport?reportFormat=' + reportFormat + '&bulletinTemplateId=' + data.Id
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

    $scope.DownloadBTSummaryReport = function (args) {

        var data = args.data;
        var reportFormat = "Excel";

        try {
            var file_src = 'IE/bulletintemplate/GetBulletinTamplateSummaryReport?reportFormat=' + reportFormat + '&bulletinTemplateId=' + data.Id
            $rootScope.report(file_src);
            //  window.open('IE/bulletintemplate/GetBulletinTamplateSummaryReport?reportFormat=' + reportFormat + '&bulletinTemplateId=' + data.Id, '_blank');

        } catch (e) {

        }
    };


    $scope.BulSummaryExcelPrints = function (args) {

        try {
            var data = args.data;
            var reportFormat = "Excel";

            var file_src = 'IE/bulletintemplate/GetBulTamplateSummaryReport?reportFormat=' + reportFormat + '&bulletinTemplateId=' + data.Id
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

    //#endregion end BulletinTamplate Reports

    //#region  MaterialSummary
    $scope.FabWidth = null;

    $scope.SetToAll = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.FabWidth)) {
                throw "Input Fabric Width.";
            }
            if (baseService.arrayLength($scope.machineOperationList) > 0) {
                for (var i = 0; i < $scope.machineOperationList.length; i++) {
                    $scope.machineOperationList[i].FabricWidth = $scope.FabWidth;
                }
            }
            $scope.FabWidth = null;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.threadMatrixList = [];
    $scope.GetMaterialSummary = function () {
        $scope.threadMatrixList = [];

        if (!baseService.isUndefinedOrNull($scope.bulletinTemplateMasterId)) {
            $http({
                method: 'GET',
                url: 'ie/bulletintemplate/GetThreadMatrixData?bulletinTemplateMasterId=' + $scope.bulletinTemplateMasterId
            }).then(function successCallback(response) {
                $scope.threadMatrixList = response.data;
            });
        }
        angular.element(document.querySelector('#MaterialSummaryPoUp')).modal('show');
    };

    $scope.MaterialSummaryRows = [{
        title: "Total", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "NeedleConsumption", dataMember: "NeedleConsumption", format: "{0:0.0000}" }
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BobbinConsumption", dataMember: "BobbinConsumption", format: "{0:0.0000}" }
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "LooperConsumption", dataMember: "LooperConsumption", format: "{0:0.0000}" }],
        showCaptionSummary: true

    }];

    $scope.GetThreadConsumptionReport = function () {
        var reportFormat = "Excel";
        try {
            var url = 'IE/bulletintemplate/GetThreadConsumptionReport?reportFormat=' + reportFormat + '&bulletinTemplateMasterId=' + $scope.bulletinTemplateMasterId + '&bulletinId=' + $scope.bulletinTemplateNew.Id;

            $rootScope.report(url);
        } catch (e) {

        }
    };

    //#endregion

    // #region checkbox all for delete multi Operation

    $scope.refreshTemplateDelOperation = function (args) {
        $("#headchk").ejCheckBox({ "change": headCheckChangeOperation });
    };

    function headCheckChangeOperation(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridBulOperation").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.operationList.length; i++) {
                $scope.operationList[i].DelFlag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridBulOperation").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.idList = [];
    $scope.sqlInStatement = null;
    $scope.CountDelItem = 0;
    function MakeMultiDeleteData() {
        $scope.CountDelItem = 0;
        for (var di = 0; di < $scope.operationList.length; di++) {
            if ($scope.operationList[di].DelFlag == true) {
                $scope.idList.push($scope.operationList[di]);
                $scope.CountDelItem++;
            }
        }

        if ($scope.idList.length > 0) {
            var uniqueMasterOrderId = removeDuplicates($scope.idList, 'Id');
            var wcEmpCode = "";
            if (uniqueMasterOrderId.length > 0) {
                wcEmpCode = "IN(";
                wcEmpCode += Array.prototype.map.call(uniqueMasterOrderId, function (item) { return "'" + item.Id + "'"; }).join(",") + ")";
            }
            $scope.sqlInStatement = wcEmpCode;
        }

    }

    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

    $scope.removeMultiOperation = function () {
        MakeMultiDeleteData();
        if (!baseService.isUndefinedOrNull($scope.sqlInStatement))
            $scope.message_multi_confirmation = 'Are you sure want to delete permanently "' + $scope.CountDelItem + '" operations.';
        angular.element(document.querySelector('#confirmMultiOperationPopUp')).modal('show');
    }

    $scope.DeleteMultiOperation = function () {
        $http({
            method: 'POST',
            url: 'IE/BulletinTemplate/DeleteMultiOperation?id=' + $scope.sqlInStatement
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.sqlInStatement = null;
                $scope.getSavedOperationData($scope.BulletinTemplateMasterId);

            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    };

    // #endregion

    //#region Bulletin Picture upload

    $scope.onBeginPBUpload = function (args) {
        try {
            if (angular.isUndefinedOrNull($scope.bulletinTemplateNew.Id))
                throw 'Please select/save the Bulletin first'

            args.data = $scope.bulletinTemplateNew.Id;
        } catch (e) {

            args.cancel = true;
            ShowResult(e, 'Error');
        }

    }
    $scope.uploadPBUrl = "IE/BulletinTemplate/SaveBulletinDefault";

    $scope.getFileList = function () {
        $http({
            method: 'POST', url: $scope.path + 'GetFileInfo', dataType: 'JSON',
            data: { Id: $scope.bulletinTemplateNew.Id }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                var str = response.data[0].PicFileName;
                var extention = str.substr(str.indexOf('.'));
                $scope.PicFileName = virtualPath.BulletinTemplateImage + '/' + $scope.bulletinTemplateNew.Id + extention;
                $scope.getData();
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }


    $scope.fileselect = function (e) {

    }
    $scope.errorPBPicUpload = function (e) {
        if (angular.isUndefinedOrNull($scope.bulletinTemplateNew.Id))
            ShowResult('Please select/save the Bulletin first', 'Error');
        else
            ShowResult("The selected file size is too large. Please select a file less than " + Math.round(e.model.fileSize / (1024 * 1024)) + "MB", 'failure');
    }

    //#endregion Bulletin Picture upload

    //#region MultiOperationCode
    $scope.MultiCodeList = [];
    $scope.AddMultiCode = function () {
        $scope.MultiCodeList = [];
        angular.element(document.querySelector('#AddMultiOperationCodePoUp')).modal('show');
    }
    $scope.CloseMultiCode = function () {
        angular.element(document.querySelector('#AddMultiOperationCodePoUp')).modal('hide');
    }
    $scope.Go = function () {

        var Sequenc = $scope.operationList.length + 1;

        var res = $scope.bulletinTemplateDetailNew.OperationCode.split(" ");
        for (var i = 0; i < res.length; i++) {
            if (checkCodeExists($scope.operationList, res[i]) === false) {
                var obj = {};
                obj.Sequenc = Sequenc + i;
                obj.OperationCode = res[i];
                $scope.MultiCodeList.push(obj);
            }
        }
        res = [];

    }
    function checkCodeExists(list, OperationCode) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].OperationCode === OperationCode) {
                return true;
            }
        }
        return false;
    }

    function checkSeqExists(list, Seq) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Sequence === Seq) {
                return true;
            }
        }
        return false;
    }

    $scope.UpdateMultiCode = function () {
        try {
            if (baseService.arrayLength($scope.MultiCodeList) == 0) {
                throw "Code is required.";
            }
            for (var i = 0; i < $scope.MultiCodeList.length; i++) {
                if (checkSeqExists($scope.operationList, $scope.MultiCodeList[i].Sequenc)) {
                    throw "This Sequence '" + $scope.MultiCodeList[i].Sequenc + "' is exists";
                }
            }

            $http({
                method: 'POST',
                url: 'IE/BulletinTemplate/InsertMultiOperation',

                data: { 'Code': $scope.bulletinTemplateDetailNew.OperationCode, 'processId': $scope.ProcessId, 'bulletinTemplateMasterId': $scope.BulletinTemplateMasterId, 'MultiCodeList': $scope.MultiCodeList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.MultiCodeList = [];
                    $scope.CloseMultiCode();
                    $scope.bulletinTemplateDetailNew.OperationCode = null;
                    $scope.getSavedOperationData($scope.BulletinTemplateMasterId);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };



        } catch (e) {
            ShowResult(e, 'failure', 'AddMultiOperationCodePoUp');
        }
    };

    //#endregion MultiOperationCode

    $scope.SOItemList = [];
    $scope.GetProductionBulletinInfo = function (obj) {
        $scope.SOItemList = [];
        $http.get('IE/BulletinTemplate/GetProductionBulletinInfo?Id=' + obj.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.SOItemList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#POItemPopup')).modal('show');
    };

    $scope.GetProdBulletinInfo = function () {
        $scope.SOItemList = [];
        $http.get('IE/BulletinTemplate/GetProductionBulletinInfo?Id=' + $scope.bulletinTemplateNew.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.SOItemList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#POItemPopup')).modal('show');
    };


}
