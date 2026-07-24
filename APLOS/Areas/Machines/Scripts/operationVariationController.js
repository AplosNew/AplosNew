'use strict';
operationVariationController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$http", "$window", "$filter"];
function operationVariationController(commonMessage, $scope, $rootScope, baseService, $http, $window, $filter) {
    $rootScope.title = "Operation Variation Master";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'Machines/operationVariation/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.operationVariations = [];

    $scope.operationVariation = {
        Id: null
        , Sequence: 0
        , OperationId: null
        , CompanyGroupId: null
        , ArticleId: null
        , ArticleName: null
        , SkillId: null
        , SkillName: null
        , SAM: 0
        , SubOperationSAM: 0
        , AdditionalSAM: 0
        , AdditionalSAMSymbol: '+'
        , Frequency: 0
        , MachineAllowance: 0
        , AdditionalAllowance: 0
        , SPI: 0
        , Code: null
        , ShortName: null
        , StandardTime: null
        , UserName: null
        , Remarks: null
        , Description: null
        , Active: true
        , isSpecialOperation: false
        , BasicProcessTime: 0
        , AssociateProcessTime: 0
        , PersonalAllowance: 0
        , IsMachineRequired: 'M'
        , TotalSAM: 0
        , StandardSPT: 0
        , OperationMasterId: null
        , OperationMasterCode: null
        , Color: null
        , AreaCode: null
        , SkillCategoryId: null
    };
    $scope.operationVariationNew = Object.assign({}, $scope.operationVariation);

    $scope.skillcategoryList = [];
    $http({
        method: 'GET',
        url: 'skills/skillcategory/getcbo'
    }).then(function successCallback(response) {
        $scope.skillcategoryList = response.data;
    });

    $scope.searchByOpStepList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Operation',
            'value': 'OperationCode'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        }
    ];

    $scope.getDataList = function () {
        baseService.init($scope.getListUrl, null, null, null, 'Sequence', 'OperationCode');
        $scope.getData = function (pageno) {
            $rootScope.parameters.operationId = $scope.operationVariationNew.OperationId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.operationVariations = result.Rows;
                    getOperationUtilityData();
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    $scope.operationList = [];

    //$http({
    //    method: 'GET',
    //    url: 'Machines/operation/getcbo'
    //}).then(function successCallback(response) {
    //    $scope.operationList = response.data;
    //});

    $scope.GetOperationData = function () {
        $http({
            method: 'GET',
            url: 'Machines/OperationVariation/GetOperationDataList'
        }).then(function successCallback(response) {
            $scope.operationList = response.data;
        });
        angular.element(document.querySelector('#MainOperationPopUp')).modal('show');
    }

    $scope.SetMainOperation = function (args) {
        //var gridObj = $("#Grid").data("ejGrid");
        //$scope.data = gridObj.getSelectedRecords()[0];
        $scope.operationVariationNew.OperationId = args.data.Id;
        $scope.operationVariationNew.OperationCode = args.data.Code;
        $scope.operationVariationNew.OperationName = args.data.UserName;
        angular.element(document.querySelector('#MainOperationPopUp')).modal('hide');
        $scope.getDataList(); $scope.GetSequence(); $scope.getAttributeList();
    }

    $scope.GetSequence = function () {
        $http.get("Machines/operationVariation/getautosequence?id=" + $scope.operationVariationNew.OperationId)
            .then(function (response) {
                $scope.operationVariationNew.Sequence = response.data;
            });
    };

    $scope.Get = function (index) {
        $scope.index = index;
        angular.copy($scope.operationVariations[$scope.index], $scope.operationVariation);
        $scope.operationVariationNew = Object.assign({}, $scope.operationVariation);
        getVairiationValue();
        calculateSAM();
        getOperationVariationSizeGroupList($scope.operationVariationNew.Id);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        if (baseService.isUndefinedOrNull($scope.operationVariationNew.AdditionalSAM)) {
            return ShowResult('Additional SAM is required.', 'failure');
        }
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.operationVariationNewForm.$valid) {
            for (var i = 0; i < $scope.attributeList.length; i++) {
                if ($scope.IsMandatoryButNull($scope.attributeList[i].IsMandatory, $scope.attributeList[i].AttributeValueFreeText)) {
                    $scope.setTab(3);
                    return ShowResult($scope.attributeList[i].OperationAttributeName + ' value is required!', 'failure');
                }
            }

            for (var i = 0; i < $scope.operationVariationSizeGroupDataList.length; i++) {
                if ($scope.operationVariationSizeGroupDataList[i].SeamLength < 0 || $scope.operationVariationSizeGroupDataList[i].SeamLength === 0) {
                    return ShowResult("Seam Length must greater than 0 for Size Group : " + $scope.operationVariationSizeGroupDataList[i].UserName + ".", 'failure');
                }
            }

            angular.copy($scope.operationVariationNew, $scope.operationVariation);
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST'
                    , url: $scope.path + 'create'
                    , data: {
                        'operationVariation': $scope.operationVariation
                        , 'valueList': $scope.attributeList
                        , 'operationVariationSizeGroupDataList': $scope.operationVariationSizeGroupDataList
                        , 'operationVariationPMDataList': $scope.operationVariationPMDataList
                    }
                    , dataStep: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getDataList();
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST'
                    , url: $scope.path + 'edit'
                    , data: {
                        'operationVariation': $scope.operationVariation
                        , 'valueList': $scope.attributeList
                        , 'operationVariationSizeGroupDataList': $scope.operationVariationSizeGroupDataList
                        , 'operationVariationPMDataList': $scope.operationVariationPMDataList
                    }
                    , dataStep: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getDataList();
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.operationVariationNew.Id)) {
            $http({
                method: 'POST',
                url: "Machines/operationVariation/delete?id=" + $scope.operationVariationNew.Id + '&operationId' + $scope.operationVariationNew.OperationId,
                dataStep: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, "failure");
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.operationVariations.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
        return true;
    };

    $scope.Clear = function () {
        $scope.attributeList = [];
        $scope.valueList = [];
        $scope.operationVariationSizeGroupDataList = [];
        $scope.operationVariationNew.Id = null;
        ClearAll($scope.GetSequence());
        $scope.operationVariationPMDataList = [];
        return true;
    };

    function ClearFields(seq) {
        $scope.attributeList = [];
        $scope.valueList = [];
        $scope.operationVariationSizeGroupDataList = [];
        $scope.operationVariationPMDataList = [];
        $scope.Action = "Save";
        $scope.operationVariation = {};
        $scope.operationVariationNew = {
            Id: null
            , OperationId: $scope.operationVariationNew.OperationId
            , Sequence: seq
            , SAM: 0
            , SubOperationSAM: 0
            , AdditionalSAM: 0
            , AdditionalSAMSymbol: '+'
            , Frequency: 0
            , MachineAllowance: 0
            , AdditionalAllowance: 0
            , SPI: 0
            , Active: true
            , isSpecialOperation: false
            , TotalSAM: 0
            , IsMachineRequired: 'M'
            , Color: null
        };
        getOperationUtilityData();
    }

    function ClearAll(seq) {
        $scope.attributeList = [];
        $scope.valueList = [];
        $scope.operationVariationSizeGroupDataList = [];
        $scope.Action = "Save";
        $scope.operationVariation = {};
        $scope.operationVariationNew = {
            Id: null
            , OperationId: $scope.operationVariationNew.OperationId
            , Sequence: seq
            , Active: true
            , isSpecialOperation: false
            , SAM: 0
            , SubOperationSAM: 0
            , AdditionalSAM: 0
            , AdditionalSAMSymbol: '+'
            , Frequency: 0
            , MachineAllowance: 0
            , AdditionalAllowance: 0
            , SPI: 0
            , IsMachineRequired: 'M'
            , TotalSAM: 0
            , Color: null
        };
        getOperationUtilityData();
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.getAttributeList = function () {
        $scope.attributeList = [];
        $scope.valueList = [];
        $http({
            method: 'GET'
            , url: 'Machines/operation/GetOperationAttributeListForSubOperation?operationId=' + $scope.operationVariationNew.OperationId
        }).then(function successCallback(response) {
            $scope.attributeList = response.data;
            for (var i = 0; i < $scope.attributeList.length; i++) {
                $scope.searchFreeField = $scope.attributeList[i].AttributeValueFreeText !== null ? true : false;
                var isFree = $scope.attributeList[i].IsFreeField;
                $scope.attributeList[i].FlagDisable = $scope.IsFreeFieldOrNot(isFree);
            }
        });
    };

    // #region Attribute Value

    $scope.valueindex = -1;
    $scope.searchvalueList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'ShortName',
            'value': 'ShortName'
        },
        {
            'name': 'StanderName',
            'value': 'StanderName'
        },
        {
            'name': 'UserName',
            'value': 'UserName'
        }
    ];
    $scope.valueParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'Code'
        , searchBy: "UserName"
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };
    $scope.attribute = null;
    $scope.valuePoUp = function (data, index) {
        $scope.attribute = data.UserName
        $scope.attributeValueUrl = 'Machines/Operation/GetValueListByAttributeId';
        baseService.setCurrentPage('valueList');
        $scope.getValueData = function (pageno) {
            $scope.valueParameters.attributeId = data.OperationAttributeId;
            baseService.paginationBase($scope.attributeValueUrl, pageno, $scope.valueParameters)
                .then(function (result) {
                    $scope.valueList = result.Rows;
                    $scope.valueParameters.total_count = result.Total;
                    $scope.valueindex = index;
                    $scope.searchFreeField = true;
                    angular.element(document.querySelector('#attributeValuePopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getValueData();
    };
    $scope.getAttrValue = function (data) {
        $scope.attributeList[$scope.valueindex].OperationAttributeValueId = data.Id;
        $scope.attributeList[$scope.valueindex].AttributeValueFreeText = data.UserName;
        $scope.attributeList[$scope.valueindex].FlagDisable = $scope.searchFreeField;
        $scope.valueindex = -1;
        angular.element(document.querySelector('#attributeValuePopUp')).modal('hide');
    };
    $scope.attributeValueClear = function (index) {
        $scope.attributeList[index].OperationAttributeValueId = null;
        $scope.attributeList[index].AttributeValueFreeText = null;
        $scope.searchFreeField = false;
        $scope.attributeList[index].FlagDisable = $scope.IsFreeFieldOrNot($scope.attributeList[index].IsFreeField);
    };
    $scope.closeValuePopUp = function () {
        angular.element(document.querySelector('#attributeValuePopUp')).modal('hide');
        CloseModalShowResult('attributeValuePopUp');
    };

    $scope.searchFreeField = false;

    $scope.IsFreeFieldOrNot = function (IsFreeField) {
        if (IsFreeField) {
            if ($scope.searchFreeField) {
                return true;//disabled true
            }
            else
                return false;//disabled false
        }
        else {
            return true;//disabled true
        }
    }
    $scope.idNullByFreeText = function (id, index) {
        if ($scope.attributeList[index].OperationAttributeId === id) {
            $scope.attributeList[index].OperationAttributeValueId = null;
        }
    };
    $scope.IsMandatoryButNull = function (isMandatory, freeText) {
        if (isMandatory) {
            if (baseService.isUndefinedOrNull(freeText)) return true;
            else return false;
        }
        else return false;
    };

    // #endregion Attribute Value

    function getOperationUtilityData() {
        $http.get('machines/operation/getOperationUtilityData?operationId=' + $scope.operationVariationNew.OperationId)
            .then(function (response) {
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

                calculateSAM();
            });
    }

    function calculateSAM() {
        var firstSam = parseFloat($scope.operationVariationNew.BasicProcessTime) + parseFloat($scope.operationVariationNew.AssociateProcessTime);
        var sam = (firstSam * $scope.operationVariationNew.PersonalAllowance / 100
            + firstSam * $scope.operationVariationNew.MachineAllowance / 100
            + firstSam * $scope.operationVariationNew.AdditionalAllowance / 100) + firstSam;
        $scope.operationVariationNew.SAM = sam.toFixed(4);
        $scope.operationVariationNew.SubOperationSAM = sam.toFixed(4);

        var total = eval(parseFloat($scope.operationVariationNew.SubOperationSAM) + $scope.operationVariationNew.AdditionalSAMSymbol + "(" + $scope.operationVariationNew.AdditionalSAM + ")");
        $scope.operationVariationNew.TotalSAM = Math.round(total * 100 + Number.EPSILON) / 100;
        // $scope.getTotalSam();
    }


    $scope.CalculateChangeSAM = function () {
        var firstSam = parseFloat($scope.operationVariationNew.BasicProcessTime) + parseFloat($scope.operationVariationNew.AssociateProcessTime);
        var sam = (firstSam * $scope.operationVariationNew.PersonalAllowance / 100
            + firstSam * $scope.operationVariationNew.MachineAllowance / 100
            + firstSam * $scope.operationVariationNew.AdditionalAllowance / 100) + firstSam;
        $scope.operationVariationNew.SAM = sam.toFixed(4);
        $scope.operationVariationNew.SubOperationSAM = sam.toFixed(4);

        var total = eval(parseFloat($scope.operationVariationNew.SubOperationSAM) + $scope.operationVariationNew.AdditionalSAMSymbol + "(" + $scope.operationVariationNew.AdditionalSAM + ")");
        $scope.operationVariationNew.TotalSAM = Math.round(total * 100 + Number.EPSILON) / 100;
        // $scope.getTotalSam();
    }

    $scope.getTotalSam = function () {
        var total = eval(parseFloat($scope.operationVariationNew.SubOperationSAM) + $scope.operationVariationNew.AdditionalSAMSymbol + "(" + $scope.operationVariationNew.AdditionalSAM + ")");

        $scope.operationVariationNew.TotalSAM = Math.round(total * 100 + Number.EPSILON) / 100;
    };

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
    $scope.materialPopUp = function (index) {
        $scope.popUpIndex = index;
        $scope.materialDataList = [];
        $scope.materialUrl = 'Materials/MaterialMaster/GetCommonMachineListByProcess?processIds=' + baseService.getColumnValueList($scope.sprocessList, 'ProcessId');
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
    $scope.articlePopUp = function (materialMasterId, materialName, materialIndex) {
        try {
            var flag = false;
            if (!baseService.isUndefinedOrNull($scope.operationVariationNew.OperationId)) {
                //var opProcessIds = $.grep($scope.operationList, function (item) { return item.Value === $scope.operationVariationNew.OperationId; })[0].ProsessIds;
                var opProcessIds = $.grep($scope.operationList, function (item) { return item.Id === $scope.operationVariationNew.OperationId; })[0].ProsessIds;

            } else {
                throw "Select Operation.";
            }

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
            $scope.operationVariationNew.MaterialName = materialName;
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

        $scope.operationVariationNew.SkillId = data.SkillId;
        $scope.operationVariationNew.SkillName = data.SkillName;
        $scope.operationVariationNew.MachineAllowance = data.MachineAllowance;
        calculateSAM();
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

    function getVairiationValue() {
        $http.get($scope.path + 'GetVairiationValue?operationId=' + $scope.operationVariationNew.OperationId + '&masterId=' + $scope.operationVariationNew.Id)
            .then(function (response) {
                $scope.attributeList = response.data;
                for (var i = 0; i < $scope.attributeList.length; i++) {
                    $scope.searchFreeField = $scope.attributeList[i].AttributeValueFreeText !== null ? true : false;
                    var isFree = $scope.attributeList[i].IsFreeField;
                    $scope.attributeList[i].FlagDisable = $scope.IsFreeFieldOrNot(isFree);
                }
            });
    }

    // #region SizeGroup

    $scope.operationVariationSizeGroupDataList = [];
    $scope.SizeGroupPopUpDataList = function () {
        $scope.sizeGroupDataList = [];
        $scope.SizeGroupSearchList = [];
        $rootScope.tempList = [];
        CloseShowResult();
        CloseModalShowResult();
        $scope.SizeGroupPopUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'Sequence'
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.SizeGroupUrl = 'IE/SizeGroup/GetList';

        baseService.setCurrentPage('sizeGroupDataList');
        $scope.GetSizeGroupDataList = function (pageno) {
            baseService.paginationBase($scope.SizeGroupUrl, pageno, $scope.SizeGroupPopUpParameters)
                .then(function (result) {
                    $scope.sizeGroupDataList = result.Rows;
                    $scope.SizeGroupPopUpParameters.total_count = result.Total;

                    if (baseService.arrayLength($scope.operationVariationSizeGroupDataList) > 0) {
                        for (var i = 0; i < $scope.operationVariationSizeGroupDataList.length; i++) {
                            for (var j = 0; j < $scope.sizeGroupDataList.length; j++) {
                                if ($scope.operationVariationSizeGroupDataList[i].SizeGroupId === $scope.sizeGroupDataList[j].Id) {
                                    $scope.sizeGroupDataList[j].Flag = true;
                                }
                            }
                        }
                    }

                    if (baseService.arrayLength($scope.gateSearchList) === 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.SizeGroupSearchList);
                    angular.element(document.querySelector('#SizeGroupPopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'SizeGroupPopUp');
                }).finally(function () {
                });
        };
        $scope.GetSizeGroupDataList();
    };

    $scope.addSizeGroup = function () {
        if (baseService.arrayLength($scope.sizeGroupDataList) > 0) {
            angular.forEach($scope.sizeGroupDataList, function (a) {
                if (checkSizeGroupExist($scope.operationVariationSizeGroupDataList, a.Id) === false) {
                    if (a.Flag) {
                        $scope.operationVariationSizeGroupDataList.push({
                            Id: null
                            , SizeGroupId: a.Id
                            , OperationVariationId: $scope.operationVariationNew.Id
                            , Sequence: a.Sequence
                            , Code: a.Code
                            , Sequence: a.Sequence
                            , ShortName: a.ShortName
                            , UserName: a.UserName
                            , StandardName: a.StandardName
                            , SeamLength: 0
                        });
                    }
                }
            });
        }
        else
            $scope.operationVariationSizeGroupDataList = [];
        angular.forEach($scope.operationVariationSizeGroupDataList, function (a) {
            if (!baseService.valueCheckInList($scope.sizeGroupDataList, 'Id', a.SizeGroupId))
                $scope.operationVariationSizeGroupDataList.splice(a, 1);
        });
        $scope.closeSizeGroupPopUp();
    };

    $scope.closeSizeGroupPopUp = function () {
        $scope.SizeGroupUpUrl = null;
        $scope.sizeGroupDataList = [];
        $scope.SizeGroupSearchList = [];
        angular.element(document.querySelector('#SizeGroupPopUp')).modal('hide');
    };

    function getOperationVariationSizeGroupList(operationVariationId) {
        $http({
            method: 'GET',
            url: 'Machines/OperationVariation/GetOperationVariationSizeGroup?operationVariationId=' + operationVariationId
        }).then(function successCallback(response) {
            $scope.operationVariationSizeGroupDataList = response.data;
            getOperationVariationPMList(operationVariationId);
        });
    }

    function checkSizeGroupExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SizeGroupId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.removeRowModal = function (index, data) {
        $scope.OperationVariationSizeGroupId = data.Id;
        $scope.bActivityIndex = index;
        if (baseService.isUndefinedOrNull($scope.OperationVariationSizeGroupId))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete permanently [ ' + data.UserName + ' ]';
        angular.element(document.querySelector('#confirmOPSZPopUp')).modal('show');
    };

    $scope.DeleteSizeGroup = function () {
        if (baseService.isUndefinedOrNull($scope.OperationVariationSizeGroupId)) {
            $scope.operationVariationSizeGroupDataList.splice($scope.bActivityIndex, 1);
        }
        else {
            $http({
                method: 'POST',
                url: 'Machines/OperationVariation/DeleteOperationVariationSizeGroup?id=' + $scope.OperationVariationSizeGroupId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.operationVariationSizeGroupDataList.splice($scope.bActivityIndex, 1);
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };

    // #endregion SizeGroup

    // #region PM

    $scope.searchPMBy = "UserName"; $scope.search = "";
    $scope.searchPMByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }];

    $scope.PMModelList = [];
    $scope.getPMData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetProductMasterList",
            data: { column: $scope.searchPMBy, value: $scope.searchPM },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PMModelList = response.data;
            angular.element(document.querySelector('#PMPopUp')).modal('show');

        });
    }

    // #region checkbox all

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridPM").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.PMModelList.length; i++) {
                $scope.PMModelList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridPM").data("ejGrid");
        gridObj.refreshContent();
    };

    // #endregion checkbox all

    $scope.operationVariationPMDataList = [];

    $scope.addPM = function () {
        MakeData();
        $scope.closePMPopUp();
    };

    function MakeData() {
        for (var i = 0; i < $scope.PMModelList.length; i++) {
            if ($scope.PMModelList[i].Flag == true) {
                if (checkPMExist($scope.operationVariationPMDataList, $scope.PMModelList[i].ProductMasterId) === false) {
                    var ob = {};
                    ob.Id = null;
                    ob.OperationVariationId = $scope.operationVariationNew.Id;
                    ob.Sequence = $scope.PMModelList[i].Sequence;
                    ob.ProductMasterId = $scope.PMModelList[i].ProductMasterId;
                    ob.Code = $scope.PMModelList[i].Code;
                    ob.Sequence = $scope.PMModelList[i].Sequence;
                    ob.ShortName = $scope.PMModelList[i].ShortName;
                    ob.UserName = $scope.PMModelList[i].UserName;
                    ob.StandardName = $scope.PMModelList[i].StandardName;
                    ob.ProductCategoryName = $scope.PMModelList[i].ProductCategoryName;
                    ob.ProductSubCategoryName = $scope.PMModelList[i].ProductSubCategoryName;
                    ob.ProductName = $scope.PMModelList[i].ProductName;
                    ob.BaseProcess = $scope.PMModelList[i].BaseProcess;
                    ob.BaseUom = $scope.PMModelList[i].BaseUom;
                    ob.Active = $scope.PMModelList[i].Active;

                    $scope.operationVariationPMDataList.push(ob);
                    ob = {};
                }
                else {
                    throw "This Product Master " + $scope.PMModelList[i].UserName + " is already taken.";
                }
            }
        }

    }

    $scope.closePMPopUp = function () {
        angular.element(document.querySelector('#PMPopUp')).modal('hide');
    };

    function getOperationVariationPMList(operationVariationId) {
        $http({
            method: 'GET',
            url: 'Machines/OperationVariation/GetOperationVariationPM?operationVariationId=' + operationVariationId
        }).then(function successCallback(response) {
            $scope.operationVariationPMDataList = response.data;
        });
    }

    function checkPMExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ProductMasterId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.removePMRowModal = function (index, data) {
        $scope.OperationVariationPMId = data.Id;
        $scope.bActivityIndex = index;
        if (baseService.isUndefinedOrNull($scope.OperationVariationPMId))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete permanently [ ' + data.UserName + ' ]';
        angular.element(document.querySelector('#confirmPMPopUp')).modal('show');
    };

    $scope.DeletePM = function () {
        if (baseService.isUndefinedOrNull($scope.OperationVariationPMId)) {
            $scope.operationVariationPMDataList.splice($scope.bActivityIndex, 1);
        }
        else {
            $http({
                method: 'POST',
                url: 'Machines/OperationVariation/DeleteOperationVariationPM?id=' + $scope.OperationVariationPMId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.operationVariationPMDataList.splice($scope.bActivityIndex, 1);
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };

    // #endregion PM

    // #region 

    $scope.OperationMasterList = [];
    $scope.Operation = null;
    $scope.showOperationPopUp = function () {
        $scope.Operation = "Operation Master";
        $http.get('employees/EmployeeInformation/GetOperationMaster')
            .then(function (response) {
                $scope.OperationMasterList = [];
                $scope.OperationMasterList = response.data;
            });

        angular.element(document.querySelector('#OperationPopUp')).modal('show');
    };

    $scope.SetOperation = function (args) {
        var gridObj = $("#Grid").data("ejGrid");
        $scope.data = gridObj.getSelectedRecords()[0];
        $scope.operationVariationNew.OperationMasterId = $scope.data.Id;
        $scope.operationVariationNew.SkillName = $scope.data.UserName;
        angular.element(document.querySelector('#OperationPopUp')).modal('hide');
        $scope.Operation = null;
    }

    // #endregion


    $scope.GetOperationVariationReportExcel = function () {
        var url = 'Machines/operationVariation/GetOperationVariationReportExcel';
        $window.open(url, '_blank');
    };
}
