'use strict';
sopItemController.$inject = ['fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function sopItemController(fileReader, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'SOP';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.sopItems = [];
    $scope.sopAttachmentDetailNewList = [];
    $scope.sopAttachmentDetailInputList = [];
    $scope.path = 'employees/sopitem/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    baseService.init($scope.getListUrl, null, null, null, 'Sequence', 'Sequence');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.sopItems = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.sopItem = {
        Id: null
        , CompanyGroupId: null
        , SOPCategoryId: null
        , SOPSubCategoryId: null
        , Sequence: null
        , Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , Objective: null
        , Mission: null
        , Vision: null
        , Description: null
        , Remarks: null
        , Active: true
    };
    $scope.sopItemNew = Object.assign({}, $scope.sopItem);

    $scope.sopAttachmentDetail = {
        Id: null,
        SOPItemId: null,
        FileName: null,
        FileId: null
    };

    $scope.activity = {
        Id: null,
        SOPItemId: null,
        PositionId: null,
        Name: null,
        ActivityDetail: null,
        PurposeOfTheActivity: null,
        ActivityCategoryId: null,
        PeriodId: null,
        Frequency: 1,
        AverageTime: null,
        ActivityImportanceId: null,
        ValueInActivity: null,
        FinancialImpact: false,
        Documents: false,
        Remarks: null,
        KPI: false
    }
    $scope.activityNew = Object.assign({}, $scope.activity);

    $scope.kpi = {
        Id: null,
        SOPActivityId: null,
        Name: null,
        Remarks: null,
        KPIDetail: null
    }
    $scope.kpiNew = Object.assign({}, $scope.kpi);

    $scope.documentActivity = {
        Id: null,
        SOPActivityId: null,
        SOPDocumentId: null
    }
    $scope.documentActivityNew = Object.assign({}, $scope.documentActivity);

    //*****************CBO*******************/

    $scope.sopCategoryList = [];
    $http({
        method: 'GET',
        url: 'Employees/sopcategory/getcbo'
    }).then(function successCallback(response) {
        $scope.sopCategoryList = response.data;
    });

    $scope.sopSubCategoryList = [];
    $http({
        method: 'GET',
        url: 'Employees/sopsubcategory/getcbo'
    }).then(function successCallback(response) {
        $scope.sopSubCategoryList = response.data;
    });

    $scope.activityCategoryList = [];
    cboService.getEnumCbo('enum/getactivitycategoryenumcbo', function (result) {
        $scope.activityCategoryList = result;
    });

    $scope.periodList = [];
    cboService.getEnumCbo('enum/getperiodenumcbo', function (result) {
        $scope.periodList = result;
    });

    $scope.activityImportanceList = [];
    cboService.getEnumCbo('enum/getactivityimportanceenumcbo', function (result) {
        $scope.activityImportanceList = result;
    });

    $scope.getActivityList = function () {
        $http({
            method: 'GET',
            url: 'employees/sopitem/getactivitycbolist?sopItemId=' + $scope.sopItemNew.Id
        }).then(function (response) {
            $scope.activitydocumentList = response.data;
            $scope.documentActivityNew.ActivityId = $scope.activityId;

        });
    };
    $scope.getKPICboList = function () {
        $http({
            method: 'GET',
            url: 'employees/sopitem/getkpicbolist?sopItemId=' + $scope.sopItemNew.Id
        }).then(function (response) {
            $scope.activitykpiList = response.data;
            $scope.kpiNew.ActivityId = $scope.activityId;
        });
    };

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.sopItemNew.Sequence = response.data;
            });
    }
    $scope.GetSequence();

    //**********************EndCbo************************/
    //var input = document.getElementById('filesToUpload');
    $scope.filedata = [];
    $('#filestoupload').change(function () {
        $scope.filedata = document.getElementById('filestoupload').files;
        var filename = document.getElementById("filestoupload").value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile").value = res;
    });

    $scope.addAttachment = function () {
        try {
            $scope.filedata = document.getElementById('filestoupload').files;
            for (var x = 0; x < $scope.filedata.length; x++) {
                //add to list
                var filName = $scope.filedata[x].name;
                if (checkFileExist($scope.sopAttachmentDetailNewList, filName)) {
                    throw filName + 'These file already added Please choose another one';
                }
                if ($scope.filedata[x].size < 1000000) {
                    $scope.sopAttachmentDetailNewList.push($scope.filedata[x]);
                }
                else {
                    $scope.ClearImage();
                    throw filName + ' File size must be below 1 mb';
                }
            }
            $scope.ClearImage();
        } catch (e) {
            throw ShowResult(e, 'failure');
        }
    };
    function checkFileExist(list, name) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].name === name) {
                return true;
            }
        }
        return false;
    }
    $scope.ClearImage = function () {
        document.getElementById('filestoupload').value = '';
        document.getElementById("uploadFile").value = '';
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.sopItem = $scope.sopItems[$scope.index];
        $scope.sopItemNew = Object.assign({}, $scope.sopItem);
        $scope.getSOPAttachmentDetail(id);
        $scope.activityData();
        $scope.getActivityList();
        $scope.getKPICboList();
        $scope.kpiDataMain();
        $scope.documentDataMain();
        //$scope.kpiData();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.getSOPAttachmentDetail = function (sopItemId) {
        $http({
            method: 'GET',
            url: $scope.path + 'getdetaillist/',
            params: { sopItemId: sopItemId }
        }).then(function (response) {
            $scope.sopAttachmentDetailNewList = response.data.Rows;
        });
    };

    $scope.documentSetIndex = -1;
    $scope.deleteDocumentSetPopup = function (ob, index) {
        try {
            $scope.documentId = ob.Id;
            $scope.deletedFileId = ob.FileId;
            $scope.deletedFileName = ob.FileName;
            //if (baseService.isUndefinedOrNull(ob.Id)) {
            //    throw 'Select a File..';
            //}
            $scope.message_confirmation = 'Are you sure want to delete [' + ob.name + ']?';
            angular.element(document.querySelector('#confirmDocumentdelete')).modal('show');
            $scope.documentSetIndex = index;
        } catch (e) {
            ShowResult(e, 'Error');
        }
        //$rootScope.passValue(_id, $scope.masterindex);
    };

    $scope.removeDocumentSetYes = function () {
        angular.element(document.querySelector('#confirmDocumentdelete')).modal('hide');
        if ($scope.documentId) {
            $scope.getSOPItemDelete();
        }
        $scope.sopAttachmentDetailNewList.splice($scope.documentSetIndex, 1);
        $scope.documentSetIndex = -1;
        //for (var i = 0; i < baseService.arrayLength($scope.wcsaveList); i++) {
        //}
    };

    $scope.getSOPItemDelete = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'sopattachmentdetaildelete',
            params: { id: $scope.documentId, fileId: $scope.deletedFileId, fileName: $scope.deletedFileName }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
    };

    $scope.Save = function () {
        angular.copy($scope.sopItemNew, $scope.sopItem);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.sopItemNewForm2.$valid) {
            try {
                var formData = new FormData();
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        headers: { 'Content-Type': undefined },
                        transformRequest: function (data) {
                            formData.append('sopItem', angular.toJson(data.sopItem));
                            for (var i = 0; i < data.file.length; i++) {
                                formData.append('file[' + i + ']', data.file[i]);
                            }
                            return formData;
                        },
                        data: { 'sopItem': $scope.sopItem, 'file': $scope.sopAttachmentDetailNewList }
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.sopItemNew.Id = response.data.SOPItem.Id;
                            $scope.getData();
                            $scope.sopItems = $filter('orderBy')($scope.sopItems, 'SOPCategory');
                            baseService.paginationAdd();
                            //ClearFields();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        headers: { 'Content-Type': undefined },
                        transformRequest: function (data) {
                            formData.append('sopItem', JSON.stringify(data.sopItem));
                            for (var i = 0; i < data.file.length; i++) {
                                formData.append('file[' + i + ']', data.file[i]);
                            }
                            return formData;
                        },
                        data: { 'sopItem': $scope.sopItem, 'file': $scope.sopAttachmentDetailNewList }
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            if ($scope.index > -1) {
                                $scope.sopItems[$scope.index] = $scope.sopItem;
                                $scope.sopItems = $filter('orderBy')($scope.sopItems, 'SOPCategory');
                            }
                            //ClearFields();
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }
            } catch (e) {
                throw ShowResult(e, 'failure');
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.sopItemNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.sopItemNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.sopItems.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    // #region Tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #endregion
    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.sopItem = {};
        $scope.sopItemNew = {};
        $scope.sopAttachmentDetailInputList = [];
        $scope.sopAttachmentDetailNewList = [];
        $scope.activityDataList = [];
        $scope.kpiList = [];
        $scope.documentsDataList = [];
        $scope.ClearImage();
        $scope.clearactivity();
        $scope.clearkpibody();
        $scope.clearDocument();
        $scope.sopItemNew = { Sequence: seq, Active: true };
    }

    $scope.FileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.SOPDocument + '/' + data.FileId + extention;
    };

    //*********************** Activity Start *************************************

    function CheckField(fieldValue, fieldName) {
        try {
            if (fieldValue == null || fieldValue == '') {
                throw ('[' + fieldName + '] is required...')
            }
        } catch (e) {
            throw e;
        }
    };

    function ValidationActivity() {
        try {
            CheckField($scope.activityNew.Name, "Activity");
            CheckField($scope.activityNew.ActivityDetail, "Activity Detail");
            CheckField($scope.activityNew.PositionId, "Position");
            CheckField($scope.activityNew.ActivityCategoryId, "Activity Category");
            CheckField($scope.activityNew.ActivityImportanceId, "Activity Importance");
            CheckField($scope.activityNew.PeriodId, "Period");
            CheckField($scope.activityNew.Frequency, "Frequency");
            CheckField($scope.activityNew.AverageTime, "Average Time");
            CheckField($scope.activityNew.ValueInActivity, "Find Value In This Activity");
            CheckField($scope.activityNew.PurposeOfTheActivity, "Purpose of the activity");
        } catch (e) {
            throw e;
        }
    };

    $scope._activityIndex = -1;
    $scope.activityCaption = 'Add Row';
    $scope.activityDataList = [];

    $scope.activityId = '';
    $scope.ActivityAction = "Save Activity";
    $scope.SaveActivity = function () {
        try {
            ValidationActivity();
            if ($scope.activityNew.Name === '' || $scope.activityNew.Name === null) {
                throw 'Please insert name';
            }
            if ($scope.activityNew.Frequency <= 0 && $scope.activityNew.Frequency === '' && $scope.activityNew.Frequency === null) {
                throw 'Frequency must greater than 0';
            }

            if ($scope.activityNew.AverageTime <= 0) {
                throw 'Average time must greater than 0';
            }

            var otherCategory = angular.element("#ActivityCategoryId :selected").text();
            if (otherCategory === 'Other (please Specify)') {
                if (baseService.isUndefinedOrNull($scope.activityNew.OtherActivityCategory)) {
                    throw 'Other Activity Category is required';
                }
            }

            var strName = $scope.activityNew.Name;
            var strActivityDetail = $scope.activityNew.ActivityDetail;
            var strRemarks = $scope.activityNew.Remarks;
            var strPurposeOfTheActivity = $scope.activityNew.PurposeOfTheActivity;
            var strOtherActivityCategory = $scope.activityNew.OtherActivityCategory;

            if (!baseService.isUndefinedOrNull($scope.activityNew.Name))
                $scope.activityNew.Name = strName.replace(/\s+/g, ' ');

            if (!baseService.isUndefinedOrNull($scope.activityNew.ActivityDetail))
                $scope.activityNew.ActivityDetail = strActivityDetail.replace(/\s+/g, ' ');

            if (!baseService.isUndefinedOrNull($scope.activityNew.Remarks))
                $scope.activityNew.Remarks = strRemarks.replace(/\s+/g, ' ');

            if (!baseService.isUndefinedOrNull($scope.activityNew.PurposeOfTheActivity))
                $scope.activityNew.PurposeOfTheActivity = strPurposeOfTheActivity.replace(/\s+/g, ' ');

            if (!baseService.isUndefinedOrNull($scope.activityNew.OtherActivityCategory))
                $scope.activityNew.OtherActivityCategory = strOtherActivityCategory.replace(/\s+/g, ' ');

            $scope.activityNew.SOPItemId = $scope.sopItemNew.Id;
            angular.copy($scope.activityNew, $scope.activity);
            if ($scope.ActivityAction === "Save Activity") {
                $scope.savebtndisable = true;
                $http({
                    method: "post",
                    url: 'employees/sopitem/saveactivity',
                    data: $scope.activity,
                    dataType: "json"
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, "failure");
                        $scope.savebtndisable = false;
                    }
                    else {
                        $scope.savebtndisable = false;
                        $scope.clearactivity();
                        //$scope.activityNew.Id = response.data.Activity.Id;
                        ShowResult(response.data.Message, "success");
                        $scope.activityData();
                        $scope.getActivityList();
                        $scope.getKPICboList();
                        $scope.documentDataMain();
                    }
                }, function errorCallback(response) {
                    $scope.savebtndisable = false;
                });
                return true;
            }
            else if ($scope.ActivityAction === "Update Activity") {
                $scope.savebtndisable = true;
                $http({
                    method: "post",
                    url: 'employees/sopitem/saveactivity',
                    data: $scope.activity,
                    dataType: "json"
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, "failure");
                        $scope.savebtndisable = false;
                    }
                    else {
                        $scope.savebtndisable = false;
                        ShowResult(response.data.Message, "success");
                        $scope.clearactivity();
                        $scope.activityData();
                        $scope.getActivityList();
                        $scope.getKPICboList();
                        $scope.documentDataMain();
                    }
                }, function errorCallback(response) {
                    $scope.savebtndisable = false;
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, "failure");
            $scope.savebtndisable = false;
        }
    };

    $scope.UpdateActivity = function (fieldName) {
        $http({
            method: "post",
            url: 'employees/sopitem/updateactivity',
            data: {
                'id': $scope.activityNew.Id,
                'fieldName': fieldName
            },
            dataType: "json"
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.activityData();
                $scope.getActivityList();
                $scope.getKPICboList();
                $scope.documentDataMain();
            }
        }, function errorCallback(response) {
        });
    };

   
    $scope.activityData = function () {
        $http({
            method: 'GET',
            url: 'employees/sopitem/getactivitylist?sopItemId=' + $scope.sopItemNew.Id
        }).then(function successCallback(response) {
            $scope.activityDataList = response.data;
        });
    }

    $scope.hidediv = false;
    $scope.activityEdit = function (data, index) {
        $scope.activityNew = Object.assign({}, data);
        $scope.activityNew.ActivityCategoryId = $scope.activityNew.ActivityCategoryId.toString();
        $scope.activityNew.PeriodId = $scope.activityNew.PeriodId.toString();
        $scope.activityNew.ActivityImportanceId = $scope.activityNew.ActivityImportanceId.toString();
        $scope.getPositionCode($scope.activityNew.PositionId);
        $scope.activityNew.ActivityCategoryId = data.ActivityCategoryId.toString();
        var text = $scope.activityNew.ActivityCategoryId;
        if (text === '6') {
            $scope.otherActivityCategory = true;
        }
        else {
            $scope.otherActivityCategory = false;
        }

        $scope._activityIndex = index;
        $scope.ActivityAction = "Update Activity";
        //$scope.hidediv = true;
    };

    $scope.DeleteActivity = function () {
        try {
            if ($scope.activityNew.Id == null || $scope.activityNew.Id == '') {
                $scope.activityDataList.splice($scope._activityIndex, 1);
                $scope._activityIndex = -1;
            }
            else {
                $http({
                    method: 'POST',
                    url: 'employees/sopitem/activitydelete',
                    dataType: 'JSON',
                    data: { 'id': $scope.activityNew.Id }
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        angular.element(document.querySelector('#confirmactivityDeletePopUp')).modal('hide');
                        deletDeleted($scope.activityNew.Id, $scope.activityDataList);
                        $scope.activityData();
                        $scope.getActivityList($scope.sopItemNew.Id);
                        $scope.getKPICboList($scope.sopItemNew.Id);
                        $scope.documentDataMain();
                        $scope.clearactivity();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.activityDelete = function (data, index) {
        $scope.activityNew.Id = data.Id;
        $scope._activityIndex = index;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + data.Name + ' ]?';
        angular.element(document.querySelector('#confirmactivityDeletePopUp')).modal('show');
    };

    function deletDeleted(id, list) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id == id) {
                list.splice(i, 1);
            }
        }
    };

    $scope.removeActivityRow = function () {
        angular.element(document.querySelector('#confirmactivityDeletePopUp')).modal('hide');
        if (baseService.isUndefinedOrNull($scope.activityNew.Id)) {
            deletDeleted($scope.activityNew.Id, $scope.activityDataList);
        }
        else {
            $scope.DeleteActivity();
        }
    };

    $scope.clearactivity = function () {
        $scope.activity = {};
        $scope.activityNew = {};
        $scope._activityIndex = -1;
        $scope.ActivityAction = "Save Activity";
        $scope.activityNew.Documents = false;
        $scope.activityNew.KPI = false;
        $scope.activityNew.Frequency = 1;
        $scope.hidediv = false;
        $scope.otherActivityCategory = false;
        //$scope.kpiList = [];
        $scope.documentsDataList = [];
        $scope.savebtndisable = false;
    };

    $scope.confirmDocument = function () {
        $scope.message_confirmation = "Does this activity have any document?";
        angular.element(document.querySelector('#document')).modal('show');
    };

    $scope.confirmCloseactivityDelete = function () {
        angular.element(document.querySelector('#confirmactivityDeletePopUp')).modal('hide');
    };

    //*********************** Activity End *************************************

    //#region *********************** KPI Start *************************************

    function ValidationKPI() {
        try {
            CheckField($scope.kpiNew.Name, "Name");
            CheckField($scope.kpiNew.KPIDetail, "KPI Detail");
        } catch (e) {
            throw e;
        }
    };

    function ValidationUpdateKPI() {
        try {
            CheckField($scope.kpiNew.SOPActivityId, "Activity ");
            CheckField($scope.kpiNew.Name, "Name");
            CheckField($scope.kpiNew.KPIDetail, "KPI Detail");
        } catch (e) {
            throw e;
        }
    };

    $scope._kpiIndex = -1;

    $scope.SaveKPI = function () {
        try {
            ValidationKPI();
            //$scope.kpiNew.SOPActivityId = $scope.activityNew.Id;
            //$scope.kpiNew.SOPItemId = $scope.sopItemNew.Id;
            $scope.savebtndisable = true;

            var strName = $scope.kpiNew.Name;
            var strRemarks = $scope.kpiNew.Remarks;
            var strKPIDetail = $scope.kpiNew.KPIDetail;

            if (!baseService.isUndefinedOrNull($scope.kpiNew.Name))
                $scope.kpiNew.Name = strName.replace(/\s+/g, ' ');

            if (!baseService.isUndefinedOrNull($scope.kpiNew.Remarks))
                $scope.kpiNew.Remarks = strRemarks.replace(/\s+/g, ' ');

            if (!baseService.isUndefinedOrNull($scope.kpiNew.KPIDetail))
                $scope.kpiNew.KPIDetail = strKPIDetail.replace(/\s+/g, ' ');

            $http({
                method: "post",
                url: 'employees/sopitem/savekpi',
                data: $scope.kpiNew,
                dataType: "json"
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure", 'kpiPopUp');
                    $scope.savebtndisable = false;
                }
                else {
                    $scope.savebtndisable = false;
                    ShowResult(response.data.Message, "success", 'kpiPopUp');
                    $scope.kpiList = [];
                    clearkpi();
                    angular.element(document.querySelector('#kpiPopUp')).modal('hide');

                    $scope.getKPICboList();
                    //$scope.kpiNew.SOPActivityId = $scope.activityNew.Id;
                    $scope.activityData();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
                $scope.savebtndisable = false;
            });
            return true;
        } catch (e) {
            ShowResult(e, "failure", 'kpiPopUp');
            $scope.savebtndisable = false;
        }
    };

    $scope.kpiDataMain = function () {
        $http.get('employees/sopitem/getkpilistmain?sopItemId=' + $scope.sopItemNew.Id)
            .then(function (response) {
                $scope.kpiList = response.data;
            })
    };

    $scope.kpiData = function () {
        $http.get('employees/sopitem/getkpilist?activityId=' + $scope.kpiNew.SOPActivityId)
            .then(function (response) {
                $scope.kpiList = response.data;
            })
    };

    $scope.KPIAction = "Save KPI";
    $scope.kpiEdit = function (data, index) {
        $scope.kpiNew = Object.assign({}, data);
        $scope._kpiIndex = index;
        $scope.KPIAction = " Update KPI";
    };

    $scope.confirmKpi = function () {
        if ($scope.conVariable === 'd') {
            $scope.conVariable = 'k';
            confirmPopUp($scope.conVariable, 'any');
        }
        else {
            $scope.conVariable = '';
            angular.element(document.querySelector('#document')).modal('hide');
            angular.element(document.querySelector('#documentPopUp')).modal('hide');
            angular.element(document.querySelector('#kpiPopUp')).modal('hide');
            $scope.documentId = null;
            $scope.activityNew.Id = null;
        }
    };

    function confirmPopUp(d, msg) {
        var message = '';
        if (d !== null || d !== undefined) {
            if (d === 'd') {
                if (!baseService.isUndefinedOrNull($scope.documentId))
                    message = 'Document Created : [' + $scope.documentId + ']<br />';
                $scope.message_confirmation = message + 'Does this activity have <b>' + msg + '</b> Document?';
            }
            else
                $scope.message_confirmation = 'Does this activity have <b>' + msg + '</b> KPI?';
            angular.element(document.querySelector('#documentPopUp')).modal('hide');
            angular.element(document.querySelector('#document')).modal('show');
        }
    };

    $scope.showDocument = function () {
        docOrKpiPopUp($scope.conVariable);
    };

    function docOrKpiPopUp(d) {
        if (d === 'd') {
            $scope.UpdateActivity('IsDocument')
            angular.element(document.querySelector('#documentPopUp')).modal('show');
        }
        else {
            $scope.UpdateActivity('IsKpi');
            angular.element(document.querySelector('#kpiPopUp')).modal('show');
        }
    };
    $scope.docOrKpi = '';

    $scope.showKPI = function () {
        $scope.docOrKpi = 'KPI';
        angular.element(document.querySelector('#kpiPopUp')).modal('show');
        angular.element(document.querySelector('#document')).modal('hide');
    };

    $scope.hideKPI = function () {
        $scope.docOrKpi = '';
        angular.element(document.querySelector('#kpiPopUp')).modal('hide');
    };

    $scope.confirmMoreKPI = function () {
        $scope.message_confirmation = "Do you have <b>more<b/> kpi for this activity?";
        angular.element(document.querySelector('#kpiPopUp')).modal('show');
        angular.element(document.querySelector('#document')).modal('hide');
    };

    $scope.closeKpi = function () {
        $scope.docOrKpi = 'KPI';
        angular.element(document.querySelector('#kpi')).modal('hide');
    };

    // #region DeleteKPI

    $scope.DeleteKpi = function () {
        try {
            if ($scope.kpiNew.Id == null || $scope.kpiNew.Id == '') {
                $scope.kpiList.splice($scope._kpiIndex, 1);
                $scope._kpiIndex = -1;
            }
            else {
                $http({
                    method: 'POST',
                    url: 'employees/sopitem/deletekpi',
                    dataType: 'JSON',
                    data: { 'id': $scope.kpiNew.Id }
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        angular.element(document.querySelector('#confirmkpiDeletePopUp')).modal('hide');
                        deletekpiDeleted($scope.kpiNew.Id, $scope.kpiList);
                        $scope.kpiDataMain();
                        //$scope.kpiData();
                        $scope.clearkpibody();
                        $scope.kpiNew.Id == null;
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.kpiDelete = function (data, index) {
        $scope.kpiNew.Id = data.Id;
        $scope._kpiIndex = index;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + data.Name + ' ]?';
        angular.element(document.querySelector('#confirmkpiDeletePopUp')).modal('show');
    };

    function deletekpiDeleted(id, list) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id == id) {
                list.splice(i, 1);
            }
        }
    };

    $scope.removeRow = function () {
        angular.element(document.querySelector('#confirmkpiDeletePopUp')).modal('hide');
        if (baseService.isUndefinedOrNull($scope.kpiNew.Id)) {
            deletekpiDeleted($scope.kpiNew.Id, $scope.kpiList);
        }
        else {
            $scope.DeleteKpi();
        }
    };

    $scope.confirmClosekpiDelete = function () {
        angular.element(document.querySelector('#confirmkpiDeletePopUp')).modal('hide');
    };
    // #endregion

    $scope.UpdateKPI = function () {
        //$scope.kpiNew.EmployeeId = $scope.employee.Id;
        try {
            ValidationUpdateKPI();
            $scope.savebtndisable = true;

            var strName = $scope.kpiNew.Name;
            var strRemarks = $scope.kpiNew.Remarks;
            var strKPIDetail = $scope.kpiNew.KPIDetail;

            if (!baseService.isUndefinedOrNull($scope.kpiNew.Name))
                $scope.kpiNew.Name = strName.replace(/\s+/g, ' ');

            if (!baseService.isUndefinedOrNull($scope.kpiNew.Remarks))
                $scope.kpiNew.Remarks = strRemarks.replace(/\s+/g, ' ');

            if (!baseService.isUndefinedOrNull($scope.kpiNew.KPIDetail))
                $scope.kpiNew.KPIDetail = strKPIDetail.replace(/\s+/g, ' ');

            $http({
                method: "post",
                url: 'employees/sopitem/savekpi',
                data: $scope.kpiNew,
                dataType: "json"
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                    $scope.savebtndisable = false;
                }
                else {
                    $scope.savebtndisable = false;
                    ShowResult(response.data.Message, "success");
                    $scope.kpiList = [];
                    $scope.clearkpibody();
                    //$scope.kpiNew.ActivityId = response.data.KPI.ActivityId;
                    $scope.activityData();
                    $scope.kpiDataMain();
                    //$scope.kpiData();
                    $scope.kpiNew.Id = null;
                    $scope.KPIAction = " Save KPI";
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
                $scope.savebtndisable = false;
            });
            return true;
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function clearkpi() {
        $scope.ActivityId = $scope.kpiNew.ActivityId;
        $scope.kpi = {};
        $scope.kpiNew = {};
        $scope.kpiNew.SOPActivityId = $scope.ActivityId;
        $scope._kpiIndex = -1;
        $scope.savebtndisable = false;
    };

    $scope.clearkpibody = function () {
        $scope.ActivityId = $scope.kpiNew.SOPActivityId;
        $scope.kpi = {};
        $scope.kpiNew = {};
        $scope.kpiNew.SOPActivityId = $scope.ActivityId;
        $scope._kpiIndex = -1;
        $scope.savebtndisable = false;
        $scope.KPIAction = " Save KPI";
    };

    //#endregion *********************** KPI End *************************************

    //*********************** Position PopUp Start *************************************
    $scope.positionSearchList = [];
    $scope.positionDataList = [];
    $scope.positionSearch = [];
    $scope.positionUrl = 'Organizations/Position/getlist';
    $scope.positionParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'UserName',
        searchBy: 'UserName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.positionPopUp = function () {
        $scope.getPositionData = function (pageno) {
            baseService.paginationBase($scope.positionUrl, pageno, $scope.positionParameters)
                .then(function (response) {
                    $scope.positionDataList = response.Rows;
                    $scope.positionParameters.total_count = response.Total;
                    if (baseService.arrayLength($scope.positionSearchList) === 0) {
                        baseService.getDDLSearchColumn($scope.positionDataList, $scope.positionSearchList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#positionPopUp')).modal('show');
        $scope.getPositionData();
    };

    $scope.closePositionPopUp = function () {
        angular.element(document.querySelector('#positionPopUp')).modal('hide');
    };

    $scope.getPositionCode = function (id) {
        $scope.positionData = [];
        $scope.positionSearch = [];
        $http({
            method: 'GET',
            url: 'Organizations/Position/get?id=' + id
        }).then(function successCallback(response) {
            $scope.positionData = [];
            $scope.positionData.push(response.data);
            baseService.getDDLSearchColumn($scope.positionData, $scope.positionSearch);
        });
    };

    $scope.selectPositionPopUp = function (data) {
        $scope.selectedPositionId = data.Id;
        $scope.activityNew.PositionId = $scope.selectedPositionId;
        $scope.activityNew.PositionName = data.UserName;
        $scope.getPositionCode($scope.selectedPositionId);
        angular.element(document.querySelector('#positionPopUp')).modal('hide');
    };

    $scope.clearPosition = function () {
        $scope.selectedPositionId = null;
        $scope.activityNew.PositionId = null;
        $scope.activityNew.PositionName = null;
        $scope.positionData = [];
        $scope.positionSearch = [];
    };
    //*********************** Position PopUp End *************************************

    //*********************** Document PopUp Start *************************************
    $scope.tempFirstList = [];
    $scope.selectFirstChValue = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempFirstList($scope.tempFirstList, data.Id) === false) {
                    $scope.tempFirstList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.tempFirstList.length; i++) {
                    if ($scope.tempFirstList[i].Id === data.Id) {
                        $scope.tempFirstList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, 'failure');
        }
    }
    function checkExistTempFirstList(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === Id) {
                return true;
            }
        }
        return false;
    }
    function getFirstActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                return true;
            }
        }
        return false;
    }
    $scope.searchByDocumentList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Document Name',
            'value': 'UserName'
        },
        {
            'name': 'Data Source Category',
            'value': 'DataSourceCategory'
        },
        {
            'name': 'File Name',
            'value': 'FileName'
        }
    ];
    $scope.documentListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: 'UserName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.documents = [];
    $scope.documentPopUp = function () {
        $scope.tempFirstList = [];
        baseService.setCurrentPage('documents');
        $scope.getDocumentData = function (pageno) {
            baseService.paginationBase('employees/sopdocument/getsopdocumentlist', pageno, $scope.documentListParameters)
                .then(function (data) {
                    $scope.documents = data.Rows;
                    $scope.documentListParameters.total_count = data.Total;
                    for (var i = 0; i < $scope.documents.length; i++) {
                        $scope.documents[i].Flag = getFirstActive($scope.tempFirstList, $scope.documents[i].Id);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#documentPopUp')).modal('show');
        $scope.getDocumentData();
    };

    $scope.selectDocument = function () {

        angular.forEach($scope.tempFirstList, function (item) {
            $scope.addDocumentSetForSave(item);
        })
        angular.element(document.querySelector('#documentPopUp')).modal('hide');
    };

    $scope.documentSetDetailList = [];
    $scope.addDocumentSetForSave = function (data) {
        if (checkDocumentExist($scope.documentSetDetailList, data.Id) === false) {
            data.DocumentId = data.Id;
            data.Id = null;
            $scope.documentSetDetailList.push(data);
        }
    }

    function checkDocumentExist(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].DocumentId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.documentSetDetailId = null;
    $scope.documentSetDetailIndex = -1;
    $scope.valuePassInDelModal = function (data, index) {
        $scope.documentSetDetailId = data.Id;
        $scope.documentSetDetailIndex = index;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + data.UserName + ' ]?';
        angular.element(document.querySelector('#confirmlngDocPopUp')).modal('show');
    };

    $scope.removeDocRow = function () {
        $scope.documentSetDetailList.splice($scope.documentSetDetailIndex, 1);
        $scope.documentSetDetailIndex = -1;
    };
    //*********************** Document PopUp End *************************************

    //*********************** SOP Document Start *************************************

    $scope.documentsDataListForSave = [];
    function documentSetDetailForSave(list) {
        angular.forEach(list, function (item) {
            $scope.documentsDataListForSave.push(
                {
                    SOPDocumentId: item.DocumentId,
                    SOPActivityId: $scope.documentActivityNew.SOPActivityId
                }
            );
        });
    }

    $scope.SaveDocument = function () {
        $scope.$broadcast('show-errors-check-validity');
        $scope.documentsDataListForSave = [];
        if (baseService.arrayLength($scope.documentSetDetailList) === 0) {
            return ShowResult('Select Document.', 'failure');
        }
        documentSetDetailForSave($scope.documentSetDetailList);
        if ($scope.documentForm.$valid) {
            $http({
                method: 'POST',
                url: 'employees/sopitem/savedocument',
                data: {
                    'document': $scope.documentsDataListForSave
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.documentSetDetailList = [];
                    $scope.documentDataMain();
                    $scope.clearDocument();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    }

    $scope.documentDataMain = function () {
        $http.get('employees/sopitem/getdocumentlistmain?sopItemId=' + $scope.sopItemNew.Id)
            .then(function (response) {
                $scope.documentsDataList = response.data;
            })
    };

    $scope.documentData = function () {
        $http.get('employees/sopitem/getdocumentlist?activityId=' + $scope.documentActivityNew.SOPActivityId)
            .then(function (response) {
                $scope.documentsDataList = response.data;
            })
    };

    $scope.DocumentAction = " Save Document";

    $scope.SOPActivityDocFileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.SOPActivityDocument + '/' + data.FileId + extention;
    };

    $scope.clearDocument = function () {
        $scope.documentSetDetailList = [];
        $scope.getActivityList();
    };

    // #region Delete Document

    $scope.DeleteDocument = function () {
        try {
            if ($scope.documentActivityNew.Id == null || $scope.documentActivityNew.Id == '') {
                $scope.documentsDataList.splice($scope._documentIndex, 1);
                $scope._documentIndex = -1;
            }
            else {
                $http({
                    method: 'POST',
                    url: 'employees/sopitem/deletedocument',
                    dataType: 'JSON',
                    data: { 'id': $scope.documentActivityNew.Id }
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        angular.element(document.querySelector('#confirmdocumentDeletePopUp')).modal('hide');
                        deleteDocumentDeleted($scope.documentActivityNew.Id, $scope.documentsDataList);
                        $scope.documentDataMain();
                        $scope.clearDocument();
                        $scope.documentActivityNew.Id == null;
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.documentDelete = function (data, index) {
        $scope.documentActivityNew.Id = data.Id;
        $scope._documentIndex = index;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + data.UserName + ' ]?';
        angular.element(document.querySelector('#confirmdocumentDeletePopUp')).modal('show');
    };

    function deleteDocumentDeleted(id, list) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id == id) {
                list.splice(i, 1);
            }
        }
    };

    $scope.removeDocumentRow = function () {
        angular.element(document.querySelector('#confirmdocumentDeletePopUp')).modal('hide');
        if (baseService.isUndefinedOrNull($scope.documentActivityNew.Id)) {
            deleteDocumentDeleted($scope.documentActivityNew.Id, $scope.documentsDataList);
        }
        else {
            $scope.DeleteDocument();
        }
    };

    $scope.confirmClosedocumentDelete = function () {
        angular.element(document.querySelector('#confirmdocumentDeletePopUp')).modal('hide');
    };
    // #endregion

    //*********************** SOP Document End *************************************
}