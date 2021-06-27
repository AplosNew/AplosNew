'use strict';
function RecipeWashMasterController(cboService, $window, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    declaration('Recipe Wash Master', 'Productions/recipewashmaster/');
    allList();
    allObject();

    ///========================================================================COMMON FUNCTION ANGULAR
    $scope.clearMMCode = function () {
        ClearMMCode();
    };
    $scope.showCharacteristicsGrid = function (hasCharForMM) {
        if (hasCharForMM == null || hasCharForMM == '') {
            return false;
        }
        else {
            return true;
        }
    }
    $scope.clearCharacteristics1Value = function () {
        $scope.mastermodal.Characteristics1ValueId = null;
        $scope.mastermodal.Characteristics1Value = null;
    };
    $scope.clearCharacteristics2Value = function () {
        $scope.mastermodal.Characteristics2ValueId = null;
        $scope.mastermodal.Characteristics2Value = null;
    };
    $scope.clearCharacteristics3Value = function () {
        $scope.mastermodal.Characteristics3ValueId = null;
        $scope.mastermodal.Characteristics3Value = null;
    };
    $scope.MainPageToModal = function () {
        for (var i in $scope.mastermodal) {
            $scope.mastermodal[i] = $scope.master[i];
        }
    }
    $scope.ModalToMainPage = function () {
        for (var i in $scope.master) {
            $scope.master[i] = $scope.mastermodal[i];
        }
    }
    $scope.getPlantCompanyWise = function () {
        try {
            if ($scope.mastermodal.CompanyId.length == 0) {
                throw "Select Company first...";
            }
            $scope.loadPlant($scope.mastermodal.CompanyId);
        } catch (e) {
            ShowResult(e, 'Error');
        }
    }
    $scope.ClearBody = function () {
        ClearOb($scope.master);
        ClearOb($scope.detailChildmodal);
        $scope.detailList = [];
        $scope.detailChildList = [];
        loadProcessList($scope.EntityId);
    }
    $scope.AddNewRecipeOperation = function () {
        $scope.recipeOperation.Id = null;
        $scope.recipeOperation.OperationId = null;
        $scope.recipeOperation.Sequence = null;
    }

    ///========================================================================LOAD LIST ANGULAR
    $scope.loadPlant = function (companyId) {
        try {
            $http.get($scope.path + "getplantcbo?companyId=" + companyId)
                .then(function (response) {
                    $scope.plantList = response.data;
                });
            $http.get($scope.path + "getunitcbo?companyId=" + companyId)
                .then(function (response) {
                    $scope.unitList = response.data;
                });
        } catch (e) {
            ShowResult(e, "Error");
        }
    };
    $scope.loadSequence = function () {
        try {
            $http.get($scope.path + 'getautosequence')
                .then(function (response) {
                    $scope.mastermodal.Sequence = response.data;
                });
        } catch (e) {
            ShowResult(e, "Error");
        }
    };
    $scope.loadDDL = function () {
        try {
            cboService.getCboCompanyByCompanyGroup(' ', function (result) {
                $scope.companyList = result;
            });
        } catch (e) {
            ShowResult(e, "Error");
        }
    };
    $scope.loadDDLDetail = function () {
        try {
            //cboService.loadSubprocessCbo($scope.ProcessId, function (result) {
            //    $scope.subProcessList = result;

            //});
            cboService.loadUtilityCbo(function (result) { $scope.utilityList = result; });
            cboService.loadUomUtilityCbo(function (result) { $scope.utilityUomList = result; });
        } catch (e) {
            ShowResult(e, "Error");
        }
    };
    $scope.loadDDLDetailChild = function () {
        try {
            $http.get($scope.path + "getmmuomcbo?materialmasterid=" + $scope.detailchildmodal.MaterialMasterId)
                .then(function (response) {
                    console.log(response.data)
                    $scope.uomChildList = response.data;
                    //$scope.detailchildmodal.BaseUOMId = BaseUOMId;
                });
        } catch (e) {
            ShowResult(e, "Error");
        }
    };
    $scope.getRawMaterialById = function (MaterialMasterId) {
        $http({
            method: 'GET',
            url: 'Materials/materialmaster/materialmasterbyid?MaterialMasterId=' + MaterialMasterId,
        }).then(function successCallback(response) {
            //console.log('kk', response.data.materialMasterData);
            if (baseService.arrayLength(response.data.materialMasterData) > 0) {
                // $scope.detailchildmodal = response.data[0];
                SetMMData($scope.detailchildmodal, response.data.materialMasterData[0])
            }
        })
    }
    $scope.getDetailData = function (masterid) {
        $http({
            method: 'GET',
            url: $scope.path + 'getdetaillist?masterid=' + masterid,
        }).then(function successCallback(response) {
            $scope.detailList = [];
            $scope.detailList = response.data;
            if (baseService.arrayLength($scope.searchbyDetaillist) == 0) {
                baseService.getDDLSearchColumn(response.data, $scope.searchbyDetaillist);
            }
        })
    }
    $scope.getUtilityData = function (recipewashsubprocessid) {
        $http({
            method: 'GET',
            url: $scope.path + 'getutilitylist?recipewashsubprocessid=' + recipewashsubprocessid,
        }).then(function successCallback(response) {
            $scope.recipeUutilityList = [];
            $scope.recipeUutilityList = response.data;
            console.log('***', $scope.recipeUutilityList);
            if (baseService.arrayLength($scope.sbrecipeUutilityList) == 0) {
                baseService.getDDLSearchColumn(response.data, $scope.sbrecipeUutilityList);
            }
        })
    }
    $scope.getOperationData = function (subprocessid) {
        $http({
            method: 'GET',
            url: $scope.path + 'getoperationlist?subprocessid=' + subprocessid,
        }).then(function successCallback(response) {
            $scope.recipeOperationList = [];
            //console.log('---',response);
            $scope.recipeOperationList = response.data;
            if (baseService.arrayLength($scope.sbrecipeOperationList) == 0) {
                baseService.getDDLSearchColumn(response.data, $scope.sbrecipeOperationList);
            }
        })
    }
    $scope.loadProcessAsperConfig = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getprocessasperconfigcbo?materialmasterid=' + $scope.master.MaterialMasterId,
        }).then(function successCallback(response) {
            $scope.processList = [];
            var r = response.data;
            if (baseService.arrayLength(r) > 0) {
                $scope.processList = r;
            }
        })
    };
    $scope.getCharacteristics = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getskuasperconfig/',
            params: { entityid: $scope.EntityId, materialmasterid: $scope.master.MaterialMasterId }
        }).then(function successCallback(response) {
            //console.log('char',response);
            ClearCharacteristics();
            if (baseService.arrayLength(response.data) > 0) {
                $scope.mastermodal.SelectedCharacteristics = response.data[0].SelectedCharacteristics;
                $scope.mastermodal.Characteristics1Selected = response.data[0].Characteristics1Selected;
                $scope.mastermodal.Characteristics2Selected = response.data[0].Characteristics2Selected;
                $scope.mastermodal.Characteristics3Selected = response.data[0].Characteristics3Selected;

                $scope.mastermodal.Characteristics1 = response.data[0].Characteristics1;
                $scope.mastermodal.Characteristics2 = response.data[0].Characteristics2;
                $scope.mastermodal.Characteristics3 = response.data[0].Characteristics3;

                $scope.mastermodal.Characteristics1Id = response.data[0].Characteristics1Id;
                $scope.mastermodal.Characteristics2Id = response.data[0].Characteristics2Id;
                $scope.mastermodal.Characteristics3Id = response.data[0].Characteristics3Id;
            }
            else {
                if ($scope.mastermodal.ProcessId != null && $scope.mastermodal.ProcessId != '') {
                    ShowResult('No data found in Recipe Config...', 'Error');
                }
            }
        })
    }
    $scope.getDetailEditData = function (pk) {
        $http({
            method: 'GET',
            url: $scope.path + 'getdetail?id=' + pk,
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.detail = response.data[0];
                $scope.detailmodal = angular.copy($scope.detail);
                //cboService.loadSubprocessCbo($scope.ProcessId, function (result) {
                //$scope.subProcessList = result;
                // cboService.loadOperationCbo($scope.detailmodal.SubprocessId, function (result) { $scope.operationList = result; });
                // });
            }
        })
    }
    $scope.getSubprocessEditData = function (pk) {
        $http({
            method: 'GET',
            url: $scope.path + 'getdetail?id=' + pk,
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                //console.log('ppp',response);
                $scope.recipeSubprocess = response.data[0];
            }
        })
    }
    $scope.getOperationEditData = function (pk) {
        $http({
            method: 'GET',
            url: $scope.path + 'getoperation?rwoid=' + pk,
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                //console.log('ppp',response);
                $scope.recipeOperation = response.data[0];
            }
        })
    }
    $scope.getUtilityEditData = function (pk) {
        $http({
            method: 'GET',
            url: $scope.path + 'getutility?recipewashutilityid=' + pk,
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.detailmodal = response.data[0];
            }
        })
    }
    $scope.getDetailChildData = function (masterid) {
        $http({
            method: 'GET',
            url: $scope.path + 'getdetailchildlist?detailid=' + masterid,
        }).then(function successCallback(response) {
            for (var i in $scope.detailchildmodal) {
                $scope.detailchildmodal[i] = null;
            }
            $scope.detailchildList = [];
            $scope.detailchildList = response.data;
            if (baseService.arrayLength($scope.searchbyDetailChildlist) == 0) {
                baseService.getDDLSearchColumn(response.data, $scope.searchbyDetailChildlist);
            }
        })
    }
    $scope.getDetailChildEditData = function (pk) {
        $http({
            method: 'GET',
            url: $scope.path + 'getdetailchild?id=' + pk,
        }).then(function successCallback(response) {
            //$scope.rawMaterial = response.data[0];
            if (baseService.arrayLength(response.data) > 0) {
                ///get mm id to get uom from db and fill cbo
                //then set uom selected value
                $scope.loadMMUomList(response.data[0]);
            }
        })
    }
    $scope.getMasterData = function (masterid) {
        $http({
            method: 'GET',
            url: $scope.path + 'getmasterlist?masterid=' + masterid,
        }).then(function successCallback(response) {
            //console.log(result);
            $scope.masterList = [];
            $scope.masterList = response.data;
            if (baseService.arrayLength($scope.masterList) > 0) {
                $scope.master = $scope.masterList[0];
                //show add detail button
                //if ($scope.master.Id != null && $scope.master.Id.length > 0) {//add edit
                //    $scope.btnDetailEntryPopup = true;
                //}//not null
            }//if length>0
        })//success
    }
    $scope.loadMMUomList = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getmmuomcbo?materialmasterid=' + $scope.master.MaterialMasterId,
        }).then(function successCallback(response) {//getmmuomcbo
            $scope.mmUomList = response.data;
        })
    };
    $scope.loadMMUomList = function (obj) {
        $scope.uomChildList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getmmuomcbo?materialmasterid=' + obj.MaterialMasterId,
        }).then(function successCallback(response) {
            $scope.uomChildList = response.data;
            $scope.rawMaterial = obj;
            $scope.getRawMaterialById(obj.MaterialMasterId);
        })
    };
    ///========================================================================LOAD SEARCH GRID ANGULAR
    $scope.getData = function () {
        baseService.init($scope.path + 'getlist', null, 25, null, 'Description', 'Description');
        $scope.loadMasterData = function (pageno) {//loadMMData
            $rootScope.parameters.MaterialMasterId = $scope.master.MaterialMasterId;

            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.masterList = result.Rows;
                    if (baseService.arrayLength($scope.searchbyMasterlist) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchbyMasterlist);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadMasterData();
    }
    $scope.getMMData = function () {
        //baseService.init($scope.path + 'getmaterialmasterlist', null, 25, null, 'Description', 'Description');
        baseService.init('Materials/materialmaster/materialmastersearch', null, 25, null, 'UserName', 'UserName');
        $scope.loadMMData = function (pageno) {//loadProcessData
            baseService.pagination(pageno)
                .then(function (result) {
                    //console.log('kk',result);
                    $scope.mmData = result.Rows;
                    if (baseService.arrayLength($scope.searchbyMaterialMasterDatalist) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchbyMaterialMasterDatalist);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadMMData();
    }
    $scope.getMMForRMData = function () {
        baseService.init('Materials/materialmaster/materialmasterrecipe', null, 25, null, 'UserName', 'UserName');
        //baseService.init($scope.path + 'MaterialMasterRecipe', null, 25, null, 'Description', 'Description');
        $scope.loadMMForRMData = function (pageno) {//loadProcessData
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.mmForRMData = result.Rows;
                    if (baseService.arrayLength($scope.searchbyMaterialMasterForRMDatalist) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchbyMaterialMasterForRMDatalist);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadMMForRMData();
    }
    $scope.getCharacteristicsValueData = function (characteristicsid) {
        //baseService.init($scope.path + 'getcharacteristicsvaluelist', null, 25, null, 'Description', 'Description');
        baseService.init('materials/characteristicsvalue/characteristicsvaluesearh', null, 25, null, 'Code', 'Code');
        $scope.loadCharacteristicsValueData = function (pageno) {//loadProcessData
            $rootScope.parameters.CharacteristicsId = characteristicsid;
            $rootScope.parameters.ids = '';
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.characteristicsValueData = result.Rows;
                    //console.log(result.Rows);
                    if (baseService.arrayLength($scope.searchbyCharacteristicsValuelist) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchbyCharacteristicsValuelist);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadCharacteristicsValueData();
    }
    ///######################################################################## SAVE AND DELETE ################################################################
    $scope.SaveMaster = function () {
        try {
            ValidationMaster();
            $scope.ModalToMainPage();
            $scope.master.ProcessId = $scope.ProcessId;
            $scope.master.EntityId = $scope.EntityId;
            //console.log($scope.mastermodal)
            //console.log($scope.master)
            $http({
                method: 'POST',
                url: $scope.path + 'createmaster',
                dataType: 'JSON',
                data: { 'master': $scope.master }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //get data by id
                    $scope.getMasterData(response.data.id)
                    //hide master entry modal
                    angular.element(document.querySelector('#masteraddeditpopup')).modal('hide');
                    //update time change the button text from update to save
                    //if ($scope.Action != 'Save') {
                    //    $scope.Action = 'Save';
                    //}
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.SaveDetail = function () {
        try {
            ValidationDetail();
            $scope.detailmodal.RecipeWashMasterId = $scope.master.Id;
            $scope.detailmodal.ProcessId = $scope.ProcessId;
            for (var i in $scope.recipeUtility) {
                $scope.recipeUtility[i] = $scope.detailmodal[i];
            }

            $scope.SaveDetailDisabled = true;
            $http({
                method: 'POST',
                url: $scope.path + 'createdetail',
                dataType: 'JSON',
                data: { 'recipeutility': $scope.recipeUtility }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    $scope.SaveDetailDisabled = false;
                    ShowResult(response.data.Message, 'failure', 'detailentrypopup');
                }
                else {
                    //angular.element(document.querySelector('#detailentrypopup')).modal('hide');
                    ShowResult(response.data.Message, 'success', 'detailentrypopup');
                    //$scope.getDetailData($scope.master.Id);
                    $scope.getUtilityData($scope.recipeUtility.RecipeWashSubprocessId);
                    // $scope.gridDetailGrid = true;
                    //angular.element(document.querySelector('#detailentrypopup')).modal('hide');
                    $scope.SaveDetailDisabled = false;
                }
            }, function errorCallback(response) {
                $scope.SaveDetailDisabled = false;
                ShowResult(response.status.Message, 'failure', 'detailentrypopup');
            });
            return true;
        } catch (e) {
            $scope.SaveDetailDisabled = false;
            ShowResult(e, 'Error', 'detailentrypopup');
        }
    }
    $scope.SaveRecipeSubprocess = function () {
        try {
            // ValidationDetail();
            $scope.recipeSubprocess.RecipeWashMasterId = $scope.master.Id;
            $scope.recipeSubprocess.ProcessId = $scope.ProcessId;
            //$scope.detailmodal.ProcessId = $scope.ProcessId;
            //$scope.master.EntityId = $scope.EntityId;
            //$scope.detailmodal.MaterialMasterId = $scope.master.MaterialMasterId;
            //for (var i in $scope.recipeSubprocess) {
            //    $scope.recipeSubprocess[i] = $scope.detailmodal[i];
            //}

            $scope.SaveDetailDisabled = true;
            $http({
                method: 'POST',
                url: $scope.path + 'CreateRecipeSubprocess',
                dataType: 'JSON',
                data: { 'recipesubprocess': $scope.recipeSubprocess }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    //angular.element(document.querySelector('#detailentrypopup')).modal('hide');
                    ShowResult(response.data.Message, 'success', 'recipesubprocessentrypopup');
                    $scope.getDetailData($scope.master.Id);
                    $scope.gridDetailGrid = true;
                    //angular.element(document.querySelector('#detailentrypopup')).modal('hide');
                    $scope.SaveDetailDisabled = false;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    }
    $scope.SaveRecipeOperation = function () {
        try {
            console.log('555', $scope.recipeOperation);
            // ValidationDetail();
            $scope.recipeOperation.RecipeWashMasterId = $scope.master.Id;
            $scope.recipeOperation.ProcessId = $scope.ProcessId;
            $scope.recipeOperation.RecipeWashSubprocessId = $scope.RecipeWashSubprocessId;
            //$scope.detailmodal.ProcessId = $scope.ProcessId;
            //$scope.master.EntityId = $scope.EntityId;
            //$scope.detailmodal.MaterialMasterId = $scope.master.MaterialMasterId;
            //for (var i in $scope.recipeSubprocess) {
            //    $scope.recipeSubprocess[i] = $scope.detailmodal[i];
            //}

            $scope.SaveDetailDisabled = true;
            $http({
                method: 'POST',
                url: $scope.path + 'CreateRecipeOperation',
                dataType: 'JSON',
                data: { 'recipeoperation': $scope.recipeOperation }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure', 'recipeoperationentrypopup');
                }
                else {
                    //angular.element(document.querySelector('#detailentrypopup')).modal('hide');
                    ShowResult(response.data.Message, 'success', 'recipeoperationentrypopup');
                    $scope.getOperationData($scope.recipeOperation.SubprocessId);
                    //$scope.getDetailData($scope.master.Id);
                    //$scope.gridDetailGrid = true;
                    //angular.element(document.querySelector('#detailentrypopup')).modal('hide');
                    $scope.SaveDetailDisabled = false;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure', 'recipeoperationentrypopup');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error', 'recipeoperationentrypopup');
        }
    }
    $scope.SaveRawMaterial = function () {
        try {
            console.log('888', $scope.rawMaterial);
            //$scope.rawMaterial.RecipeWashSubprocessId = $scope.RecipeWashSubprocessId;

            $scope.rawMaterial.RecipeWashMasterId = $scope.master.Id;
            $scope.rawMaterial.RecipeWashSubprocessId = $scope.detailmodal.RecipeWashSubprocessId;
            $scope.rawMaterial.SubprocessId = $scope.detailmodal.SubprocessId;

            //$scope.rawMaterial.ProcessId = $scope.ProcessId;
            // $scope.rawMaterial.RecipeSubprocessId = $scope.ProcessId;
            $scope.rawMaterial.MaterialMasterId = $scope.detailchildmodal.MaterialMasterId;
            ValidationDetailChild();
            $scope.SaveDetailChildDisabled = true;
            $http({
                method: 'POST',
                //url: $scope.saveUrlDetailChild,
                url: $scope.path + 'CreateRawMaterial',
                dataType: 'JSON',
                data: { 'recipewashrawmaterial': $scope.rawMaterial }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    //angular.element(document.querySelector('#detailentrypopup')).modal('hide');
                    ShowResult(response.data.Message, 'success');
                    $scope.getDetailChildData($scope.rawMaterial.UtilityId);
                    $scope.SaveDetailChildDisabled = false;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    }

    $scope.DeleteMaster = function () {
        try {
            $scope.master.Id = $scope.mastermodal.Id;
            if ($scope.master.Id == null || $scope.master.Id == '') {
                throw 'No Recipe is found...';
            }
            $http({
                method: 'POST',
                url: $scope.path + 'deletemaster',
                dataType: 'JSON',
                data: { 'masterid': $scope.master.Id }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    angular.element(document.querySelector('#masteraddeditpopup')).modal('hide');
                    $scope.masterAddEditPopup('DELETE');
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    }
    $scope.DeleteDetail = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'deletedetail',
            dataType: 'JSON',
            data: { 'detailid': $scope.detailmodal.Id }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                //other child
                $scope.getDetailData($scope.master.Id);
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    }
    $scope.DeleteOperation = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'deleterecipeoperation',
            dataType: 'JSON',
            data: { 'operationid': $scope.recipeOperation.Id }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure', 'recipeoperationentrypopup');
            }
            else {
                ShowResult(response.data.Message, 'success', 'recipeoperationentrypopup');
                $scope.getOperationData($scope.recipeOperation.SubprocessId);
                $scope.AddNewRecipeOperation();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure', 'recipeoperationentrypopup');
        });
        return true;
    }
    $scope.DeleteUtility = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'deleterecipeutility',
            dataType: 'JSON',
            data: { 'utilityid': $scope.recipeUtility.Id }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure', 'recipeutilityentrypopup');
            }
            else {
                ShowResult(response.data.Message, 'success', 'recipeutilityentrypopup');
                $scope.getUtilityData($scope.recipeUtility.RecipeWashSubprocessId);
                //$scope.getOperationData($scope.recipeOperation.SubprocessId);
                //$scope.AddNewRecipeOperation();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure', 'recipeutilityentrypopup');
        });
        return true;
    }
    $scope.DeleteRawMaterial = function () {
        $http({
            method: 'POST',
            // url: $scope.deleteUrlDetailChild,
            url: $scope.path + 'deleterawmaterial',
            dataType: 'JSON',
            data: { 'rawmaterialid': $scope.rawMaterial.Id }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure', 'recipewashrawmaterialpopup');
            }
            else {
                ShowResult(response.data.Message, 'success', 'recipewashrawmaterialpopup');
                //reload other child
                $scope.getDetailChildData($scope.rawMaterial.UtilityId);
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure', 'recipewashrawmaterialpopup');
        });
        return true;
    }
    ///========================================================================SEARCH POPUP
    $scope.masterSearchPopup = function () {
        $scope.getData();
        angular.element(document.querySelector('#mastersearchpopup')).modal('show');
    };
    $scope.showProcessModal = function () {
        $scope.getProcessData();
        angular.element(document.querySelector('#processmodal')).modal('show');
    };
    $scope.showMMRMModal = function () {
        $scope.getMMForRMData();
        angular.element(document.querySelector('#mmrmmodal')).modal('show');
    };
    $scope.showMMModal = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.ProcessId)) {
                throw "Process can not be blank...";
            }
            $scope.getMMData();
            angular.element(document.querySelector('#mmmodal')).modal('show');
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.searchCharacteristics3Value = function (cvid) {
        $scope.dim = "3";
        $scope.getCharacteristicsValueData(cvid);
        angular.element(document.querySelector('#characteristicsValuepopup')).modal('show');
    };
    $scope.searchCharacteristics2Value = function (cvid) {
        $scope.dim = "2";
        $scope.getCharacteristicsValueData(cvid);
        angular.element(document.querySelector('#characteristicsValuepopup')).modal('show');
    };
    $scope.searchCharacteristics1Value = function (cvid) {
        $scope.dim = "1";
        $scope.getCharacteristicsValueData(cvid);
        angular.element(document.querySelector('#characteristicsValuepopup')).modal('show');
    };
    ///========================================================================ENTRY POPUP
    $scope.masterAddEditPopup = function (flag) {
        try {
            if (flag == 'NEW') {
                ClearMasterModal();
                LoadProcessCriteria();//by monir
                LoadUom(flag);
                $scope.getCharacteristics();
                angular.element(document.querySelector('#masteraddeditpopup')).modal('show');
            }
            else if (flag == 'DELETE') {
                ClearMaster();
                angular.element(document.querySelector('#masteraddeditpopup')).modal('hide');
            }
            else {
                if (baseService.arrayLength($scope.detailList) > 0) {
                    throw "Line Item (Subprocess) is available, so edition is not possible...";
                }
                LoadUom(flag);
                ClearMasterModal();
                $scope.getCharacteristics();
                LoadProcessCriteria();
                //$scope.MainPageToModal();
                angular.element(document.querySelector('#masteraddeditpopup')).modal('show');
            }
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.detailEntryPopup = function (ob, flag) {
        if ($scope.master.Id == null || $scope.master == "") {
            ShowResult("Select a 'Master' first....")
            return;
        }

        //if ($scope.master.ProcessId == null || $scope.master.ProcessId == "") {
        //    ShowResult("'Process' is not selected....")
        //    return;
        //}
        ClearDetailModal();
        ////$scope.detailindex = -1;
        //$scope.SaveDetailDisabled = false;
        //$scope.CancelDetail();
        $scope.loadDDLDetail();
        cboService.getWashOperationCbo(ob.Id, function (result) { $scope.operationList = result; });
        if (flag == 'NEW') {
            $scope.detailchildList = [];
            //$scope.detailmodal = angular.copy($scope.detail);
            for (var i in $scope.detailmodal) {
                $scope.detailmodal[i] = null;
            }
            $scope.detailmodal.RecipeWashSubprocessId = ob.Id;
            $scope.detailmodal.SubprocessId = ob.SubprocessId;
            //$scope.ActionDetail = 'Save';
        }
        else {
            $scope.getDetailEditData(ob.Id);
        }
        angular.element(document.querySelector('#detailentrypopup')).modal('show');
    };
    $scope.showRawMaterialPopup = function (ob) {
        $scope.rawMaterial.RecipeWashOperationId = ob.RecipeWashOperationId
        $scope.rawMaterial.RecipeWashUtilityId = ob.Id
        $scope.rawMaterial.OperationId = ob.OperationId
        $scope.rawMaterial.UtilityId = ob.UtilityId
        $scope.getDetailChildData($scope.rawMaterial.UtilityId);
        angular.element(document.querySelector('#recipewashrawmaterialpopup')).modal('show');
    };
    $scope.showSubprocessPopup = function (id, flag) {
        try {
            if ($scope.master.Id.length == 0) {
                throw "Please Select a recipe...";
            }

            if (flag == 'EDIT') {
                $scope.recipeSubprocess.SubprocessId = null;
                $scope.recipeSubprocess.Sequence = null;
                $scope.recipeSubprocess.RecipeWashMasterId = $scope.master.Id;
                ///load ddl subprocess and then data by id and set selected
                cboService.loadSubprocessCbo($scope.ProcessId, function (result) {
                    $scope.subProcessList = result;
                    $scope.getSubprocessEditData(id);
                });
            }
            else {
                $scope.recipeSubprocess.Id = null;
                $scope.recipeSubprocess.SubprocessId = null;
                $scope.recipeSubprocess.Sequence = null;
                $scope.recipeSubprocess.RecipeWashMasterId = $scope.master.Id;
                cboService.loadSubprocessCbo($scope.ProcessId, function (result) {
                    $scope.subProcessList = result;
                });
            }
            angular.element(document.querySelector('#recipesubprocessentrypopup')).modal('show');
            //$scope.getDetailData($scope.master.Id);
        } catch (e) {
            ShowResult(e, 'Error');
        }
    }
    $scope.showOperationPopup = function (ob, flag) {
        try {
            if (baseService.isUndefinedOrNull(ob.Id)) {
                throw "Please Select a Subprocess...";
            }
            $scope.getOperationData(ob.SubprocessId);
            $scope.RecipeWashSubprocessId = ob.Id;
            $scope.recipeOperation.SubprocessId = ob.SubprocessId;

            if (flag == 'EDIT') {
                $scope.recipeOperation.Sequence = null;
                $scope.recipeOperation.OperationId = null;
                $scope.recipeOperation.RecipeWashMasterId = $scope.master.Id;
                ///load ddl subprocess and then operation and then data by id and set selected
                //cboService.loadSubprocessCbo($scope.ProcessId, function (result) {
                //    $scope.subProcessList = result;
                cboService.loadOperationCbo(ob.SubprocessId, function (result) {
                    $scope.operationList = result;
                    // $scope.getOperationEditData(id);
                });
                //});
            }
            else {
                $scope.recipeOperation.Id = null;
                //$scope.recipeOperation.SubprocessId = ob.SubprocessId;
                $scope.recipeOperation.OperationId = null;
                $scope.recipeOperation.Sequence = null;
                //$scope.recipeOperation.RecipeWashSubprocessId = ob.Id;
                $scope.recipeOperation.RecipeWashMasterId = $scope.master.Id;
                //cboService.loadSubprocessCbo($scope.ProcessId, function (result) {
                //    $scope.subProcessList = result;
                cboService.loadOperationCbo(ob.SubprocessId, function (result) {
                    $scope.operationList = result;
                });
                //});
            }
            //console.log('বাংলা', $scope.recipeOperation);
            angular.element(document.querySelector('#recipeoperationentrypopup')).modal('show');
            //$scope.getDetailData($scope.master.Id);
        } catch (e) {
            ShowResult(e, 'Error');
        }
    }
    ///========================================================================DELETE POPUP
    $scope.deleteMaster = function () {
        var _id = $scope.mastermodal.Id;
        $scope.message_confirmation = "Are you sure to delete [" + _id + "] ";
        angular.element(document.querySelector('#confirmmasterdelete')).modal('show');
        //$rootScope.passValue(_id, $scope.masterindex);
    }
    $scope.removeMasterYes = function () {
        angular.element(document.querySelector('#confirmmasterdelete')).modal('hide');
        $scope.DeleteMaster();
    };
    $scope.removeRowYes = function () {
        $scope.DeleteDetail();
        angular.element(document.querySelector('#detailentrypopup')).modal('hide');
    };
    $scope.deleteDetailGrid = function (id) {
        $scope.detailmodal.Id = id;
        $scope.message_confirmation = "Are you sure to delete [" + id + "] ";
        angular.element(document.querySelector('#confirmdetaildelete')).modal('show');
    }
    $scope.removeRowDetailYes = function () {
        $scope.DeleteDetail();
        angular.element(document.querySelector('#confirmdetaildelete')).modal('hide');
    };
    $scope.deleteOperation = function (id) {
        $scope.recipeOperation.Id = id;
        $scope.message_confirmation = "Are you sure to delete [" + id + "] ";
        angular.element(document.querySelector('#coperationdelete')).modal('show');
    }
    $scope.removeOperationYes = function () {
        $scope.DeleteOperation();
        angular.element(document.querySelector('#coperationdelete')).modal('hide');
    };
    $scope.deleteUtility = function (ob) {
        $scope.recipeUtility.Id = ob.Id;
        ///to reload list after deletion.
        $scope.recipeUtility.RecipeWashSubprocessId = ob.RecipeWashSubprocessId;
        $scope.message_confirmation = "Are you sure to delete [" + ob.Utility + "] ";
        angular.element(document.querySelector('#cutilitydelete')).modal('show');
    }
    $scope.removeUtilityYes = function () {
        $scope.DeleteUtility();
        angular.element(document.querySelector('#cutilitydelete')).modal('hide');
    };
    $scope.deleteRawMaterial = function (ob) {
        $scope.rawMaterial.Id = ob.Id;
        $scope.message_confirmation = "Are you sure to delete [" + ob.MaterialMaster + "] ";
        angular.element(document.querySelector('#crmdelete')).modal('show');
    }
    $scope.removeRawMaterialYes = function () {
        $scope.DeleteRawMaterial();
        angular.element(document.querySelector('#crmdelete')).modal('hide');
    };
    ///######################################################################### SELECTED ROW #########################################################################
    $scope.getProcessCode = function (id, code) {
        $scope.mastermodal.ProcessId = id;
        $scope.mastermodal.Process = code;
        angular.element(document.querySelector('#processmodal')).modal('hide');
    };
    $scope.clearProcessCode = function (id, code) {
        $scope.mastermodal.ProcessId = null;
        $scope.mastermodal.Process = null;
    };
    $scope.GetMasterIndex = function (id) {
        //$scope.masterindex = index;
        //$scope.master = $scope.masterList[$scope.masterindex];
        //console.log($scope.master);
        $scope.getMasterData(id);
        $scope.getDetailData(id);
        //$scope.btnDetailEntryPopup = true;
        // $scope.bulletinmastermodal = $scope.bulletinmasterList[$scope.masterindex];
        angular.element(document.querySelector('#mastersearchpopup')).modal('hide');
    };
    $scope.getMMCode = function (obj) {
        //mmid, Code, Description, MaterialGridId, BaseUOM, BaseUOMId
        SetMMData($scope.master, obj);
        SetMMData($scope.mastermodal, obj);
        angular.element(document.querySelector('#mmmodal')).modal('hide');
    };
    $scope.rawMaterialSingle = function (id) {
        try {
            if (id == null || id == "") {
                ShowResult("Select a 'Line Item' first....")
                return;
            }
            $scope.rawMaterial.Id = id;
            // $scope.CancelDetailChild();
            $scope.getDetailChildEditData(id);
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.getDetailRow = function (ob, falg) {
        $scope.detailEntryPopup(ob, falg);
        $scope.getUtilityData(ob.Id);
    }
    $scope.getDetailChildRow = function (index) {
        $scope.detailChildEntryPopup('EDIT');
        $scope.detailchildmodal = $scope.detailChildList[index];
    }
    $scope.getMMRMCode = function (ob) {
        //mmid, Code, Description, MaterialGridId, BaseUOM, BaseUOMId
        ClearDetailChild($scope.detailchildmodal);
        SetMMData($scope.detailchildmodal, ob)
        $scope.loadDDLDetailChild();
        angular.element(document.querySelector('#mmrmmodal')).modal('hide');
    };
    $scope.getCharacteristicsValueCode = function (id, Description) {
        if ($scope.dim == "1") {
            if (id == null || id == '') {
                $scope.mastermodal.Characteristics1ValueId = null;
                $scope.mastermodal.Characteristics1Value = null;
                // ShowResult('This Material Master has no Grid ...', 'Information');
            }
            else {
                $scope.mastermodal.Characteristics1ValueId = id;
                $scope.mastermodal.Characteristics1Value = Description;
            }
        }
        else if ($scope.dim == "2") {
            if (id == null || id == '') {
                $scope.mastermodal.Characteristics2ValueId = null;
                $scope.mastermodal.Characteristics2Value = null;
                // ShowResult('This Material Master has no Grid ...', 'Information');
            }
            else {
                $scope.mastermodal.Characteristics2ValueId = id;
                $scope.mastermodal.Characteristics2Value = Description;
            }
        }
        else if ($scope.dim == "3") {
            if (id == null || id == '') {
                $scope.mastermodal.Characteristics3ValueId = null;
                $scope.mastermodal.Characteristics3Value = null;
                // ShowResult('This Material Master has no Grid ...', 'Information');
            }
            else {
                $scope.mastermodal.Characteristics3ValueId = id;
                $scope.mastermodal.Characteristics3Value = Description;
            }
        }
        angular.element(document.querySelector('#characteristicsValuepopup')).modal('hide');
    };
    ///==========================================================================LOAD FROM DATABASE
    function LoadUom(flag) {
        $http({
            method: 'GET',
            url: $scope.path + 'getmmuomcbo?materialmasterid=' + $scope.master.MaterialMasterId,
        }).then(function successCallback(response) {//getmmuomcbo
            $scope.uomList = response.data;
            $scope.avgUomList = response.data;
            if (flag == 'EDIT') {
                $scope.MainPageToModal();
            }
        })
    };
    function LoadProcessCriteria() {
        $http({
            method: 'GET',
            url: 'Processes/processcriteria/getcriteriacbo'
        }).then(function (response) {
            $scope.processCriteriaList = response.data;
            console.log(' $scope.processCriteriaList', response.data);
        });
    }
    function loadProcessList(entityid) { cboService.GetEntityProcessCbo(entityid, function (result) { $scope.processList = result; }); }
    ///==========================================================================COMMON FUNCTION
    function ClearOb(obj) {
        for (var i in obj) {
            obj[i] = null;
        }
    }
    function ClearObject(obj) {
        for (var i in obj) {
            obj[i] = null;
        }
    }
    function ClearDetailChild() {
        //list obj savebtn savetext
        ClearObject($scope.detailchildmodal);
        $scope.SaveDetailChildDisabled = false;
        $scope.ActionDetailChild = 'Save'
        //$scope.detailchildList = [];
        $scope.uomChildList = [];
    }
    function allList() {
        $scope.detailList = [];
        $scope.detailchildList = [];
        $scope.subProcessList = [];
        $scope.utilityList = [];
        $scope.recipeUutilityList = [];
        $scope.recipeOperationList = [];

        $scope.sbrecipeOperationList = [];
        $scope.searchbyDetaillist = [];
        $scope.searchbyDetailChildlist = [];
        $scope.searchbyMasterlist = [];
        $scope.searchbyMaterialMasterDatalist = [];
        $scope.searchbyMaterialMasterForRMDatalist = [];
        $scope.searchbyCharacteristicsValuelist = [];
        $scope.sbrecipeUutilityList = [];

        $scope.companyList = [];
        $scope.plantList = [];
        $scope.mmUomList = [];
        $scope.uomChildList = [];

        $scope.departmentList = [];
        $scope.lineList = [];
        $scope.subsectionList = [];
        $scope.sectionList = [];
        $scope.divisionList = [];
        $scope.characteristicsValueData = [];
    }
    function allObject() {
        $scope.recipeSubprocess = {
            Id: null,
            RecipeWashMasterId: null,
            SubprocessId: null,
            Sequence: null
        };
        $scope.recipeOperation = {
            Id: null,
            RecipeWashMasterId: null,
            RecipeWashSubprocessId: null,
            SubprocessId: null,
            OperationId: null,
            Sequence: null
        };
        $scope.recipeUtility = {
            Id: null,
            RecipeWashMasterId: null,
            RecipeWashSubprocessId: null,
            RecipeWashOperationId: null,
            SubprocessId: null,
            OperationId: null,
            UtilityId: null,
            Temperature: null,
            IsFixed: 'Fixed',
            Ph: null,
            QtyValue: null,
            Uom: null,
            Duration: null,
            Remark: null,
            Sequence: null
        };
        $scope.detailmodal = {
            Id: null,
            RecipeWashMasterId: null,
            RecipeWashSubprocessId: null,
            RecipeWashOperationId: null,
            SubprocessId: null,
            OperationId: null,
            UtilityId: null,
            LiquorRatio1: null,
            LiquorRatio2: null,
            Temperature: null,
            IsPercentage: null,
            Ph: null,
            Qty: null,
            Uom: null,
            Duration: null,
            Remark: null,
            Sequence: null
        };
        $scope.rawMaterial = {
            Id: null,
            RecipeWashMasterId: null,
            RecipeWashSubprocessId: null,
            RecipeWashOperationId: null,
            RecipeWashUtilityId: null,
            SubprocessId: null,
            OperationId: null,
            UtilityId: null,
            MaterialMasterId: null,
            UomId: null,
            QtyValue: null,
            IsFixed: 'Fixed',
            IsOperationLevel: null,
            Remark: null
        };
        $scope.detailchildmodal = {
            Id: null,
            RecipeWashMasterId: null,
            RecipeWashSubprocessId: null,
            RecipeWashOperationId: null,
            RecipeWashUtilityId: null,
            SubprocessId: null,
            OperationId: null,
            UtilityId: null,
            MaterialMasterId: null,
            UomId: null,
            Qty: null,
            IsPercentage: null,
            IsOperationLevel: null,
            Remark: null
        };

        $scope.master = {
            Id: null,
            Description: null,
            MaterialMasterDescription: null,
            MaterialMasterCode: null,
            MaterialMasterId: null,
            Code: null, Uom: null, AvgUom: null, MaterialAvgWeight: null,
            UserName: null,
            BatchSize: null,
            ProcessId: null, ProcessCriteriaId: null,
            Characteristics1Selected: true,
            Characteristics2Selected: true,
            Characteristics3Selected: true,
            Characteristics1: null,
            Characteristics2: null,
            Characteristics3: null,
            Characteristics1Id: null,
            Characteristics2Id: null,
            Characteristics3Id: null,
            Characteristics1ValueId: null,
            Characteristics2ValueId: null,
            Characteristics3ValueId: null,
            Characteristics1Value: null,
            Characteristics2Value: null,
            Characteristics3Value: null,
            EndTemperature: null,
            StartTemperature: null,
            StartPressure: null,
            EndPressure: null,
            GradientTemperature: null,
            GradientPressure: null,
            Process: null, EntityId: null,
            SelectedCharacteristics: null,
        };
        $scope.mastermodal = {
            Id: null,
            Description: null,
            MaterialMasterDescription: null,
            MaterialMasterCode: null,
            MaterialMasterId: null,
            Code: null, Uom: null, AvgUom: null, MaterialAvgWeight: null,
            UserName: null,
            BatchSize: null,
            ProcessId: null, ProcessCriteriaId: null,
            Characteristics1Selected: true,
            Characteristics2Selected: true,
            Characteristics3Selected: true,
            Characteristics1: null,
            Characteristics2: null,
            Characteristics3: null,
            Characteristics1Id: null,
            Characteristics2Id: null,
            Characteristics3Id: null,
            Characteristics1ValueId: null,
            Characteristics2ValueId: null,
            Characteristics3ValueId: null,
            Characteristics1Value: null,
            Characteristics2Value: null,
            Characteristics3Value: null,
            EndTemperature: null,
            StartTemperature: null,
            StartPressure: null,
            EndPressure: null,
            GradientTemperature: null,
            GradientPressure: null,
            Process: null, EntityId: null,
            SelectedCharacteristics: null,
        };
    }
    function declaration(title, path) {
        $rootScope.title = title
        $scope.path = path;
        $scope.message_confirmation = "";
        $scope.SaveDetailDisabled = false;
        $scope.SaveDetailChildDisabled = false;
        $scope.RecipeWashSubprocessId = null;
        $scope.IsProportionate = false;
    }
    function ClearMMCode() {
        ClearOb($scope.master);
        ClearOb($scope.mastermodal);
        ClearOb($scope.recipeSubprocess);
        ClearOb($scope.recipeOperation);
        ClearOb($scope.recipeUtility);
        ClearOb($scope.detailmodal);
        ClearOb($scope.rawMaterial);
        ClearOb($scope.detailchildmodal);
        allList();
        $scope.RecipeWashSubprocessId = null;
    }
    function CheckField(fieldValue, fieldName) {
        try {
            if (fieldValue == null || fieldValue == '') {
                throw ('[' + fieldName + '] is required...')
            }
        } catch (e) {
            throw e;
        }
    }
    function CheckFieldTime(fieldValue, fieldName) {
        try {
            CheckField(fieldValue, fieldName);
            if (fieldValue.length != 5) {
                throw fieldName + ' is not correct format...Ex: 08:00, 15:30 (HH:mm)';
            }
            if (fieldValue.substr(2, 1) != ':') {
                throw fieldName + ' is not correct format...Ex: 08:00, 15:30 (HH:mm)';
            }
            var a = parseInt(fieldValue.substr(0, 2));
            if (a > 23) {
                throw fieldName + ' can not be greater than 23...';
            }
            if (a < 0) {
                throw fieldName + ' can not be negetive...';
            }
            var b = parseInt(fieldValue.substr(3, 2));
            if (b > 59) {
                throw fieldName + ' can not be greater than 59...';
            }
            if (b < 0) {
                throw fieldName + ' can not be negetive...';
            }

            if (a == 0 && b == 0) {
                throw fieldName + ' can not be blank...';
            }
            //first 2 digit check integer
            //last 2 digit check integer
        } catch (e) {
            throw e;
        }
    }
    function ValidationMaster() {
        try {
            //check PORecipeTag
            CheckField($scope.mastermodal.Code, 'Code');
            CheckField($scope.mastermodal.UserName, 'UserName');
            CheckField($scope.mastermodal.MaterialAvgWeight, 'Avg Weight');
            CheckField($scope.mastermodal.AvgUom, 'Avg Uom');
            CheckField($scope.mastermodal.BatchSize, 'BatchSize');
            CheckField($scope.mastermodal.Uom, 'Uom');

            if ($scope.mastermodal.Characteristics1Selected) {
                CheckField($scope.mastermodal.Characteristics1ValueId, $scope.mastermodal.Characteristics1);
            }
            if ($scope.mastermodal.Characteristics2Selected) {
                CheckField($scope.mastermodal.Characteristics2ValueId, $scope.mastermodal.Characteristics2);
            }
            if ($scope.mastermodal.Characteristics3Selected) {
                CheckField($scope.mastermodal.Characteristics3ValueId, $scope.mastermodal.Characteristics3);
            }
        } catch (e) {
            throw e;
        }
    }
    function ValidationDetail() {
        try {
            CheckField($scope.master.Id, 'Recipe Master');
            CheckField($scope.detailmodal.SubprocessId, 'Subprocess');
            CheckField($scope.detailmodal.RecipeWashOperationId, 'Operation');
            CheckField($scope.detailmodal.UtilityId, 'Utility');
            CheckField($scope.detailmodal.QtyValue, 'Value');
            CheckField($scope.detailmodal.Ph, 'Ph');
            CheckField($scope.detailmodal.Duration, 'Duration');
            CheckField($scope.detailmodal.Temperature, 'Temperature');
            CheckField($scope.detailmodal.Remark, 'Remark');
            //CheckField($scope.detailmodal.SectionId, 'Section');
            //CheckField($scope.detailmodal.SubsectionId, 'Subsection');
            //CheckField($scope.detailmodal.LineId, 'Line');

            // CheckDuplicateSubprocess($scope.detailmodal);
        } catch (e) {
            throw e;
        }
    }
    function CheckDuplicateSubprocess(ob) {
        try {
            for (var i = 0; i < baseService.arrayLength($scope.detailList); i++) {
                if (ob.Id != $scope.detailList[i].Id) {
                    if (ob.SubprocessId == $scope.detailList[i].SubprocessId) {
                        throw "Subprocess: [" + $scope.detailList[i].Subprocess + "] has already been taken...";
                    }//id
                }//id
            }
        } catch (e) {
            throw e;
        }
    }
    function ClearCharacteristics() {
        $scope.mastermodal.SelectedCharacteristics = null;
        $scope.mastermodal.Characteristics1Selected = null;
        $scope.mastermodal.Characteristics2Selected = null;
        $scope.mastermodal.Characteristics3Selected = null;

        $scope.mastermodal.Characteristics1 = null;
        $scope.mastermodal.Characteristics2 = null;
        $scope.mastermodal.Characteristics3 = null;

        $scope.mastermodal.Characteristics1Id = null;
        $scope.mastermodal.Characteristics2Id = null;
        $scope.mastermodal.Characteristics3Id = null;
    }
    function ValidationDetailChild() {
        try {
            //
            //CheckField($scope.detailchildmodal.RecipeSubprocessId, 'Line Item (RecipeSubprocessId) is not selected...');
            //CheckField($scope.detailchildmodal.MaterialMasterId, 'Material Master');
            //CheckField($scope.detailchildmodal.Qty, 'Qty');
            //var _qty = $scope.detailchildmodal.Qty;
            //if (_qty <= 0) {
            //    throw "Qty must be greater than Zero...";
            //}
            //CheckUOMandPerc();
            //CheckDuplicate($scope.detailchildmodal);
        } catch (e) {
            throw e;
        }
    }
    function CheckUOMandPerc() {
        try {
            if ($scope.detailchildmodal.Ispercentage) {
                if ($scope.detailchildmodal.UomId != null && $scope.detailchildmodal.UomId != '') {
                    throw ('UOM and Percentage both can not be taken...')
                }
            }
            else {
                CheckField($scope.detailchildmodal.UomId, 'UOM');
            }
        } catch (e) {
            throw e;
        }
    }
    function CheckDuplicate(ob) {
        try {
            for (var i = 0; i < arrayLength($scope.detailchildList); i++) {
                if (ob.Id != $scope.detailchildList[i].Id) {
                    if (ob.MaterialMasterId == $scope.detailchildList[i].RawMaterialId) {
                        throw "Material Master: [" + ob.MaterialMasterDescription + "] has already been taken...";
                    }//id
                }//id
            }
        } catch (e) {
            throw e;
        }
    }
    function SetMMData(list, obj) {
        list.MaterialMasterId = obj.Id;
        list.MaterialMasterDescription = obj.Description;
        list.MaterialMasterCode = obj.Code;
        list.UserName = obj.UserName;
        list.MaterialType = obj.MaterialType;
        list.MaterialGroup = obj.MaterialGroupMaster;
        list.GridNO = obj.GridName;
        list.MaterialGridId = obj.MaterialGridId;
        list.BaseUOM = obj.BaseUom;
        list.BaseUOMId = obj.BaseUOMId;
    }
    function ClearMasterModal() {
        for (var i in $scope.mastermodal) {
            if (i != 'MaterialMasterId' && i != 'MaterialMasterDescription' && i != 'MaterialMasterCode') {
                $scope.mastermodal[i] = null;
            }
        }
        $scope.btnDetailEntryPopup = true;
        $scope.btndeletemaster = false;
        $scope.Action = 'Save';
    }
    function ClearMaster() {
        for (var i in $scope.master) {
            if (i != 'MaterialMasterId' && i != 'MaterialMasterDescription' && i != 'MaterialMasterCode') {
                $scope.master[i] = null;
            }
        }
        ClearMasterModal();
        ClearDetail();
    }
    function ClearDetail() {
        //ClearObject($scope.detailmodal);
        $scope.detailList = [];
        ClearObject($scope.detail);
        $scope.gridDetailGrid = false;
        $scope.btnDetailEntryPopup = false;
        ClearDetailModal();
        ClearDetailChild();
    }
    function ClearDetailModal() {
        ClearObject($scope.detailmodal);
        // $scope.SaveDetailDisabled = false;
        // $scope.ActionDetail = 'Save'
        $scope.subProcessList = [];
    }

    ///==========================================================================LOAD TIME CALL
    cboService.getCboProductionEntityByCompany($window.companyGroupId, $window.companyId, function (result) {
        $scope.entityList = result;
    });
};
RecipeWashMasterController.$inject = ["cboService", "$window", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];