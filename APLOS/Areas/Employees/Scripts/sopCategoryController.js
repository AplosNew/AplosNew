'use strict';
SOPCategoryController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function SOPCategoryController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "SOP Category";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.sopCategories = [];
    $scope.path = 'Employees/SOPCategory/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.sopCategories = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.SOPCategory = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        FileName: null,
        FileId: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.sopCategoryNew = Object.assign({}, $scope.SOPCategory);

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.sopCategoryNew.Sequence = response.data;
            });
    }
    $scope.GetSequence();
    $scope.Get = function (id, index) {
        $scope.filedata = {};
        $scope.index = index;
        $scope.SOPCategory = $scope.sopCategories[$scope.index];
        $scope.sopCategoryNew = Object.assign({}, $scope.SOPCategory);
        var filename = document.getElementById("uploadFile").value = $scope.SOPCategory.FileName;
        $scope.filedata.name = $scope.SOPCategory.FileName;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    //File Attachment-----Start

    $scope.filedata = null;
    $("#uploadBtn").change(function () {
        $scope.filedata = this.files[0];
    });

    $scope.FileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.SOPCategory + '/' + data.FileId + extention;
    };

    document.getElementById("uploadBtn").onchange = function () {
        var filename = document.getElementById("uploadFile").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile").value = res;
    };

    $scope.DocumentRemove = function () {
        $scope.message_confirmation = 'Are you sure to remove this file?';
        angular.element(document.querySelector('#confirmDocumentDelete')).modal('show');
    };
    $scope.removeDocument = function () {
        angular.element(document.querySelector('#confirmDocumentDelete')).modal('hide');
        document.getElementById('uploadBtn').value = '';
        $scope.filedata = '';
        $scope.SOPCategory.FileName = "";
        $scope.filedata = {};
        document.getElementById('uploadFile').value = "";
        $scope.getData();
    };

    $scope.ClearDocument = function () {
        document.getElementById('uploadBtn').value = '';
        $scope.filedata = '';
        $scope.SOPCategory.FileName = "";
        $scope.filedata = {};
        document.getElementById('uploadFile').value = "";
    };

    $scope.confirmCloseDocumentDelete = function () {
        angular.element(document.querySelector('#confirmDocumentDelete')).modal('hide');
    };

    //File Attachment-----End

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.sopCategoryNewForm.$valid) {

            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb.';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;
            $scope.sopCategoryNew.FileName = fileName;
            if (!baseService.isUndefinedOrNull($scope.sopCategoryNew.FileName)) {
                if ($scope.sopCategoryNew.FileName.length > 50) {
                    throw "File Name must be less than 50 character."
                }
            }
            var formData = new FormData();

            angular.copy($scope.sopCategoryNew, $scope.SOPCategory);
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        formData.append("SOPCategory", angular.toJson(data.SOPCategory));
                        if (baseService.isUndefinedOrNull($scope.filedata) == false) {
                            formData.append('file', data.file);
                        }
                        return formData;
                    },
                    data: { 'SOPCategory': $scope.SOPCategory, 'file': $scope.filedata }
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.sopCategories.push(response.data.SOPCategory);
                        $scope.sopCategories = $filter('orderBy')($scope.sopCategories, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                        $scope.ClearDocument();
                        $scope.getData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        formData.append("SOPCategory", angular.toJson(data.SOPCategory));
                        if (baseService.isUndefinedOrNull($scope.filedata) == false) {
                            formData.append('file', data.file);
                        }
                        return formData;
                    },
                    data: { 'SOPCategory': $scope.SOPCategory, 'file': $scope.filedata }
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.sopCategories[$scope.index] = $scope.SOPCategory;
                            $scope.sopCategories = $filter('orderBy')($scope.sopCategories, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                        $scope.ClearDocument();
                        $scope.getData();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.sopCategoryNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.sopCategoryNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.sopCategories.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                    $scope.ClearDocument();
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        $scope.ClearDocument();
        return true;
    }
    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.SOPCategory = {};
        $scope.sopCategoryNew = {};
        $scope.sopCategoryNew.Sequence = seq;
        $scope.sopCategoryNew.Active = true;
    }
};