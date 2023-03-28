'use strict';
DocumentSetupController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function DocumentSetupController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Document Setup';

    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #region Document Category
    $rootScope.titleDC = "Document Category";
    $scope.ActionDC = 'Save';
    $scope.indexDC = -1;
    $scope.DocumentCategoryList = [];
    $scope.pathDC = 'employees/DocumentCategory/';
    $scope.getListUrlDC = $scope.pathDC + 'getlist';
    $scope.getSeqUrlDC = $scope.pathDC + 'getautosequence';
    $scope.saveUrlDC = $scope.pathDC + 'create';
    $scope.updateUrlDC = $scope.pathDC + 'edit';
    $scope.deleteUrlDC = $scope.pathDC + 'delete/';
    baseService.init($scope.getListUrlDC, null, null, null, "Sequence", "UserName");

    $scope.getDataDC = function () {
        $http({
            method: 'POST',
            url: $scope.pathDC + "GetList",
            data: { column: $scope.searchByDC, value: $scope.searchDC },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.DocumentCategoryList = response.data;
        });
    }
    $scope.getDataDC();

    $scope.document = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };
    $scope.documentNew = Object.assign({}, $scope.document);

    $scope.GetSequenceDC = function () {
        $http.get($scope.getSeqUrlDC)
            .then(function (response) {
                $scope.documentNew.Sequence = response.data;
            });
    };
    $scope.GetSequenceDC();

    $scope.GetDC = function (args) {
        $scope.documentNew = Object.assign({}, args.data);
        /* $scope.GetActivity(args.data.Id);*/
        /*$scope.getActivityGridData($scope.ModelNew.Id);*/
        $scope.ActionDC = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveDC = function () {
        $http({
            method: 'POST',
            url: $scope.saveUrlDC,
            data: { 'data': $scope.documentNew },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                ClearFieldsDC(response.data.Sequence);
                $scope.getDataDC();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };


    $scope.DeleteDC = function () {
        if (!baseService.isUndefinedOrNull($scope.documentNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrlDC + $scope.documentNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsDC(response.data.Sequence);
                    $scope.getDataDC();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.RemoveDC = function () {

        if (!baseService.isUndefinedOrNull($scope.ModelNewDC.Id))
            $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDetailPopUpDC')).modal('show');
    }

    $scope.ClearDC = function () {
        ClearFieldsDC($scope.GetSequenceDC());
        return true;
    };

    function ClearFieldsDC(seq) {
        $scope.ActionDC = "Save";
        $scope.document = {};
        $scope.documentNew = {};
        $scope.documentNew.Sequence = seq;
        $scope.documentNew.Active = true;
    }
    // #endregion Document Category

    // #region Document Sub Category
    $rootScope.titleDSC = "SOP Document SubCategory";
    $scope.ActionDSC = 'Save';
    
    $scope.sopDocumentSubCategories = [];
    $scope.pathDSC = 'Employees/SOPDocumentSubCategory/';
    $scope.getListUrlDSC = $scope.pathDSC + 'GetList';
    $scope.getSeqUrlDSC = $scope.pathDSC + 'getautosequence';
    $scope.saveUrlDSC = $scope.pathDSC + 'create';
    $scope.updateUrlDSC = $scope.pathDSC + 'edit';
    $scope.deleteUrlDSC = $scope.pathDSC + 'delete/';
    baseService.init($scope.getListUrlDSC);


    $scope.getDataDSC = function () {
        $http({
            method: 'POST',
            url: $scope.getListUrlDSC,
            data: { column: $scope.searchByDSC, value: $scope.searchDSC },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.sopDocumentSubCategories = response.data;
            ClearFieldsDSC(response.data.Sequence);
            $scope.GetSequenceDSC();
        });
    }
    $scope.getDataDSC();

    $scope.SOPDocumentSubCategory = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true                                                                                   
    };
    $scope.sopDocumentSubCategoryNew = Object.assign({}, $scope.SOPDocumentSubCategory);

   
    $scope.GetSequenceDSC = function () {
        cboService.getSequence($scope.getSeqUrlDSC, function (data) {
            $scope.SOPDocumentSubCategory.Sequence = data;
            $scope.sopDocumentSubCategoryNew.Sequence = data;
        });
    };
    $scope.GetSequenceDSC();

    $scope.GetDSC = function (args) {

        $scope.sopDocumentSubCategoryNew = Object.assign({}, args.data);
        $scope.ActionDSC = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveDSC = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.sopDocumentSubCategoryNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlDSC,
                data: { 'data': $scope.sopDocumentSubCategoryNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsDSC(response.data.Sequence);
                    $scope.getDataDSC();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    }
    $scope.DeleteDSC = function () {
        if (!baseService.isUndefinedOrNull($scope.sopDocumentSubCategoryNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrlDSC + $scope.sopDocumentSubCategoryNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.sopDocumentSubCategories.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFieldsDSC(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }

    $scope.RemoveDSC = function () {

        if (!baseService.isUndefinedOrNull($scope.ModelNewDS.Id))
            $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDetailPopUpDSC')).modal('show');
    }

    $scope.ClearDSC = function () {
        ClearFieldsDSC($scope.GetSequenceDSC());
        return true;
    }
    function ClearFieldsDSC(seq) {
        $scope.ActionDSC = "Save";
        //$scope.SOPDocumentSubCategory = {};
        //$scope.sopDocumentSubCategoryNew = {};
       
        

        $scope.sopDocumentSubCategoryNew = Object.assign({}, $scope.SOPDocumentSubCategory);
        $scope.sopDocumentSubCategoryNew.Sequence = seq;
        $scope.sopDocumentSubCategoryNew.Active = true;
    }
    // #endregion Document Sub Category

    // #region Document Source
    $rootScope.titleDS = 'Document Source';
    $scope.ActionDS = 'Save';
    $scope.ModelListDS = [];
    $scope.pathDS = 'QMS/DocumentSource/';
    $scope.getListUrlDS = $scope.pathDS + 'getlist';
    $scope.getSeqUrlDS = $scope.pathDS + 'getautosequence';
    $scope.saveUrlDS = $scope.pathDS + 'create';
    $scope.deleteUrlDS = $scope.pathDS + 'delete/';
    baseService.init($scope.getListUrlDS);
    $scope.searchByDS = "UserName"; $scope.searchDS = "";
    $scope.searchByListDS = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getDataDS = function () {
        $http({
            method: 'POST',
            url: $scope.pathDS + "GetList",
            data: { column: $scope.searchByDS, value: $scope.searchDS },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelListDS = response.data;
            ClearFieldsDS(response.data.Sequence);
            $scope.GetSequenceDS();
        });
    }
    $scope.getDataDS();

    $scope.ModelTempDS = {
        Id: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        Sequence: 0
    };
    $scope.ModelNewDS = Object.assign({}, $scope.ModelTempDS);

    $scope.GetSequenceDS = function () {
        cboService.getSequence($scope.getSeqUrlDS, function (data) {
            $scope.ModelTempDS.Sequence = data;
            $scope.ModelNewDS.Sequence = data;
        });
    };
    $scope.GetSequenceDS();

    $scope.GetDS = function (args) {

        $scope.ModelNewDS = Object.assign({}, args.data);
        $scope.ActionDS = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveDS = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewDSForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlDS,
                data: { 'data': $scope.ModelNewDS },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsDS(response.data.Sequence);
                    $scope.getDataDS();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DeleteDS = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNewDS.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrlDS + $scope.ModelNewDS.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsDS(response.data.Sequence);
                    $scope.getDataDS();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.RemoveDS = function () {

        if (!baseService.isUndefinedOrNull($scope.ModelNewDS.Id))
            $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDetailPopUpDS')).modal('show');
    }

    $scope.ClearDS = function () {
        ClearFieldsDS($scope.GetSequenceDS());
        return true;
    };

    function ClearFieldsDS(seq) {
        $scope.Action = 'Save';
        $scope.ModelNewDS = Object.assign({}, $scope.ModelTempDS);
        $scope.ModelNewDS.Sequence = seq;
    }
    // #endregion Document Source

    // #region Document Type
    $rootScope.titleDT = 'Document Type';
    $scope.ActionDT = 'Save';
    $scope.ModelListDT = [];
    $scope.pathDT = 'QMS/DocumentType/';
    $scope.getListUrlDT = $scope.pathDT + 'getlist';
    $scope.getSeqUrlDT = $scope.pathDT + 'getautosequence';
    $scope.saveUrlDT = $scope.pathDT + 'create';
    $scope.deleteUrlDT = $scope.pathDT + 'delete/';
    baseService.init($scope.getListUrlDT);
    $scope.searchByDT = "UserName"; $scope.searchDT = "";
    $scope.searchByListDT = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getDataDT = function () {
        $http({
            method: 'POST',
            url: $scope.pathDT + "GetList",
            data: { column: $scope.searchByDT, value: $scope.searchDT },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetSequenceDT();
        });
    }
    $scope.getDataDT();

    $scope.ModelTempDT = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        Archive: false
    };
    $scope.ModelNewDT = Object.assign({}, $scope.ModelTempDT);

    $scope.GetSequenceDT = function () {
        cboService.getSequence($scope.getSeqUrlDT, function (data) {
            $scope.ModelTempDS.Sequence = data;
            $scope.ModelNewDS.Sequence = data;
        });
    };
    $scope.GetSequenceDT();

    $scope.GetDT = function (args) {

        $scope.ModelNewDT = Object.assign({}, args.data);
        $scope.ActionDT = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveDT = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewDTForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlDT,
                data: { 'data': $scope.ModelNewDT },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsDT(response.data.Sequence);
                    $scope.getDataDT();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DeleteDT = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNewDT.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrlDT + $scope.ModelNewDT.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsDT(response.data.Sequence);
                    $scope.getDataDT();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };
    $scope.message_Detailconfirmation = null;
    $scope.RemoveDT = function () {

        if (!baseService.isUndefinedOrNull($scope.ModelNewDT.Id))
            $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDetailPopUpDT')).modal('show');
    }


    $scope.ClearDT = function () {
        ClearFieldsDT($scope.GetSequenceDT());
        return true;
    };

    function ClearFieldsDT(seq) {
        $scope.ActionDT = 'Save';
        $scope.ModelNewDT = Object.assign({}, $scope.ModelTempDT);
        $scope.ModelNewDT.Sequence = seq;
    }
    // #endregion Document Type

    // #region Document Location
     $rootScope.title = 'Document Location';
    $scope.ModelList = [];
    $scope.DslModelList = [];
    $scope.path = 'qms/documentlocation/';

    $scope.getListUrl = $scope.path + 'getlist';
    $scope.dslgetListUrl = $scope.path + 'getlistdsl';

    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getSeqUrldsl = $scope.path + 'getautosequencedsl';

    $scope.saveUrl = $scope.path + 'create';
    $scope.dslsaveUrl = $scope.path + 'createdsl';

    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.dsldeleteUrl = $scope.path + 'deletedsl/';

    baseService.init($scope.getListUrl);


    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.dslsearchBy = "UserName"; $scope.dslsearch = "";

    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];
    $scope.dslsearchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetSequence();
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        $scope.GetSequenceDsl($scope.ModelNew.Id);
        $scope.getDataDsl($scope.ModelNew.Id);
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();

        }
    };
    $scope.Action = 'Save';

    // To show data in grid
    $scope.Getgrid = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;

        });
    }

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
                    $scope.getDataDsl($scope.ModelNew.Id);
                    $scope.Action = 'Update';
                    $scope.Getgrid();
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
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
        $scope.DslModelList = [];
        $scope.GetSequenceDsl($scope.ModelNew.Id);
        $scope.getDataDsl($scope.ModelNew.Id);
    }


    // pop up

    $scope.Popup = function () {
        angular.element(document.querySelector('#SublocationPoUp')).modal('show');
    }

    $scope.CloseProcess = function () {
        ClearFieldsDsl($scope.GetSequenceDsl($scope.ModelNew.Id));
        angular.element(document.querySelector('#SublocationPoUp')).modal('hide');
    }

    // Document Sub location
    $scope.DslModelList = [];
    $scope.getDataDsl = function (DocumentLocationId) {
        $http({
            method: 'POST',
            url: $scope.path + "GetListDsl?DocumentLocationId=" + DocumentLocationId,
            data: { column: $scope.dslsearchBy, value: $scope.dslsearch },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DslModelList = response.data;

        });
    }

    $scope.DSLModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        DocumentLocationId: null
    };
    $scope.DSLModelNew = Object.assign({}, $scope.DSLModelTemp);


    $scope.GetSequenceDsl = function (DocumentLocationId) {
        $http.get("qms/DocumentLocation/GetAutoSequenceDsl?DocumentLocationId=" + DocumentLocationId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.DSLModelNew.Sequence = response.data[0].Sequence;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
   


    $scope.GetDsl = function (args) {
        $scope.Popup();
        $scope.DSLModelNew = Object.assign({}, args.data);
    
        $scope.ActionDSL = 'Update';

        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.ActionDSL = 'Save';



    $scope.SaveDsl = function () {
        $scope.DSLModelNew.DocumentLocationId = $scope.ModelNew.Id;
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.DSLModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.dslsaveUrl,
                data: { 'data': $scope.DSLModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsDsl(response.data.Sequence);
                    $scope.getDataDsl($scope.ModelNew.Id);
                    $scope.GetSequenceDsl($scope.ModelNew.Id);

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.DeleteDsl = function () {
        if (!baseService.isUndefinedOrNull($scope.DSLModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.dsldeleteUrl + $scope.DSLModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsDsl(response.data.Sequence);
                    $scope.getDatadsl($scope.ModelNew.Id);
                    $scope.GetSequenceDsl($scope.ModelNew.Id);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };


    $scope.ClearDsl = function () {
        ClearFieldsDsl($scope.GetSequenceDsl($scope.ModelNew.Id));
        return true;
    };

    function ClearFieldsDsl(seq) {
        $scope.ActionDSL = 'Save';
        $scope.DSLModelNew = Object.assign({}, $scope.DSLModelTemp);
        $scope.DSLModelNew.Sequence = seq;
        $scope.getDataDsl($scope.ModelNew.Id);
        $scope.GetSequenceDsl($scope.ModelNew.Id);
    }
    // #endregion Document Location
}