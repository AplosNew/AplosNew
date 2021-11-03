'use strict';
jobDescriptionController.$inject = ['fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function jobDescriptionController(fileReader, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Job Description Category';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.jobDescriptions = [];
    $scope.jobDescriptionDetailNewList = [];
    $scope.jobDescriptionDetailInputList = [];
    $scope.path = 'employees/jobdescription/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'JobDescriptionCategoryName', 'JobDescriptionCategoryName');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.jobDescriptions = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    $scope.searchByList = [
        {
            'name': 'Category',
            'value': 'JobDescriptionCategoryName'
        },
        {
            'name': 'Subcategory',
            'value': 'JobDescriptionSubCategory'
        },
        {
            'name': 'Item',
            'value': 'JobDescriptionItem'
        },
        {
            'name': 'Job Level',
            'value': 'JobLevel'
        },
        {
            'name': 'Primary/Secondary',
            'value': 'PrimaryOrSecondary'
        },
        {
            'name': 'Frequency',
            'value': 'Frequency'
        }
    ];

    $scope.jobDescription = {
        Id: null,
        CompanyGroupId: null,
        JobDescriptionCategoryId: null,
        JobDescriptionSubCategoryId: null,
        JobDescriptionItemId: null,
        JobLevel: null,
        PrimaryOrSecondary: null,
        Frequency: null,
        NatureOfActivity: null,
        SystemOrManual: null,
        DocumentApplicable: false,
        EstimatedTimeRequired: null
    };

    $scope.jobDescriptionDetail = {
        Id: null,
        JobDescriptionId: null,
        FileName: null,
        FileId: null
    };
    $scope.jobDescriptionNew = Object.assign({}, $scope.jobDescription);
    //*****************CBO*******************/
    $scope.jobDescriptionCategoryList = [];
    $scope.jobDescriptionItemList = [];
    $scope.jobDescriptionPrimaryOrSecondaryList = [];
    $scope.jobDescriptionNatureOfActivityList = [];
    $scope.jobDescriptionSubCategoryList = [];
    $scope.levelList = [];
    $scope.frequencyList = [];
    $scope.systemOrManualList = [];
    cboService.getEnumCbo('enum/GetJobDescriptionLevelListCbo', function (result) {
        $scope.levelList = result;
    });

    cboService.getEnumCbo('enum/GetJobDescriptionFrequencyListCbo', function (result) {
        $scope.frequencyList = result;
    });

    cboService.getEnumCbo('enum/GetJobDescriptionPrimaryOrSecondaryListCbo', function (result) {
        $scope.jobDescriptionPrimaryOrSecondaryList = result;
    });

    cboService.getEnumCbo('enum/GetJobDescriptionNatureOrActivityListCbo', function (result) {
        $scope.jobDescriptionNatureOfActivityList = result;
    });

    cboService.getEnumCbo('enum/GetJobDescriptionSystemOrManualListCbo', function (result) {
        $scope.systemOrManualList = result;
    });

    cboService.jobDescriptionCategoryList(function (result) {
        $scope.jobDescriptionCategoryList = result;
    });

    cboService.jobDescriptionSubCategoryList(function (result) {
        $scope.jobDescriptionSubCategoryList = result;
    });

    cboService.jobDescriptionItemList(function (result) {
        $scope.jobDescriptionItemList = result;
    });

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
                if (checkFileExist($scope.jobDescriptionDetailNewList, filName)) {
                    throw filName + 'These file already added Please choose another one';
                }
                if ($scope.filedata[x].size < 1000000) {
                    $scope.jobDescriptionDetailNewList.push($scope.filedata[x]);
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
        $scope.jobDescription = $scope.jobDescriptions[$scope.index];
        $scope.jobDescriptionNew = Object.assign({}, $scope.jobDescription);
        $scope.getJobDescriptionDetail(id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.getJobDescriptionDetail = function (jobDescriptionId) {
        $http({
            method: 'GET',
            url: $scope.path + 'getdetaillist/',
            params: { jobDescriptionId: jobDescriptionId }
        }).then(function (response) {
            $scope.jobDescriptionDetailNewList = response.data.Rows;
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
            $scope.getJobDescriptionDelete();
        }
        $scope.jobDescriptionDetailNewList.splice($scope.documentSetIndex, 1);
        $scope.documentSetIndex = -1;
        //for (var i = 0; i < baseService.arrayLength($scope.wcsaveList); i++) {

        //}
    };

    $scope.getJobDescriptionDelete = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'jobdescriptiondetaildelete',
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
        angular.copy($scope.jobDescriptionNew, $scope.jobDescription);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.jobDescriptionNewForm.$valid) {
            try {
                if ($scope.jobDescriptionNew.DocumentApplicable && $scope.jobDescriptionDetailNewList.length < 1) {
                    throw 'Please Select at least one file!';
                }
                if ($scope.jobDescriptionNew.DocumentApplicable === false && $scope.jobDescriptionDetailNewList.length > 0) {
                    throw 'Please Select Document Applicable!';
                }
                var formData = new FormData();
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        headers: { 'Content-Type': undefined },
                        transformRequest: function (data) {
                            formData.append('jobDescription', angular.toJson(data.jobDescription));
                            for (var i = 0; i < data.file.length; i++) {
                                formData.append('file[' + i + ']', data.file[i]);
                            }
                            return formData;
                        },
                        data: { 'jobDescription': $scope.jobDescription, 'file': $scope.jobDescriptionDetailNewList }
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getData();
                            $scope.jobDescriptions = $filter('orderBy')($scope.jobDescriptions, 'JobDescriptionCategory');
                            baseService.paginationAdd();
                            ClearFields();
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
                            formData.append('jobDescription', angular.toJson(data.jobDescription));
                            for (var i = 0; i < data.file.length; i++) {
                                formData.append('file[' + i + ']', data.file[i]);
                            }
                            return formData;
                        },
                        data: { 'jobDescription': $scope.jobDescription, 'file': $scope.jobDescriptionDetailNewList }
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            if ($scope.index > -1) {
                                $scope.jobDescriptions[$scope.index] = $scope.jobDescription;
                                $scope.jobDescriptions = $filter('orderBy')($scope.jobDescriptions, 'JobDescriptionCategory');
                            }
                            ClearFields();
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
        if (!baseService.isUndefinedOrNull($scope.jobDescriptionNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.jobDescriptionNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.jobDescriptions.splice($scope.index, 1);
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
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.jobDescription = {};
        $scope.jobDescriptionNew = {};
        $scope.jobDescriptionDetailInputList = [];
        $scope.jobDescriptionDetailNewList = [];
        $scope.ClearImage();
        $scope.jobDescriptionNew.Active = true;
    }

    $scope.FileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = $rootScope.EmployeeJobDescription + '/' + data.FileId + extention;
    };
}