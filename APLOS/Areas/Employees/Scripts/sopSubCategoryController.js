'use strict';
SOPSubCategoryController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function SOPSubCategoryController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "SOP SubCategory";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.sopSubCategories = [];
    $scope.path = 'Employees/SOPSubCategory/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.sopSubCategories = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.SOPSubCategory = {
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
    $scope.sopSubCategoryNew = Object.assign({}, $scope.SOPSubCategory);

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.sopSubCategoryNew.Sequence = response.data;
            });
    }
    $scope.GetSequence();
    $scope.Get = function (id, index) {
        $scope.filedata = {};
        $scope.index = index;
        $scope.SOPSubCategory = $scope.sopSubCategories[$scope.index];
        $scope.sopSubCategoryNew = Object.assign({}, $scope.SOPSubCategory);
        var filename = document.getElementById("uploadFile").value = $scope.SOPSubCategory.FileName;
        $scope.filedata.name = $scope.SOPSubCategory.FileName;
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
        $scope.dwonloadUrl = virtualPath.SOPSubCategory + '/' + data.FileId + extention;
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
        $scope.SOPSubCategory.FileName = "";
        $scope.filedata = {};
        document.getElementById('uploadFile').value = "";
        $scope.getData();
    };

    $scope.ClearDocument = function () {
        document.getElementById('uploadBtn').value = '';
        $scope.filedata = '';
        $scope.SOPSubCategory.FileName = "";
        $scope.filedata = {};
        document.getElementById('uploadFile').value = "";
    };

    $scope.confirmCloseDocumentDelete = function () {
        angular.element(document.querySelector('#confirmDocumentDelete')).modal('hide');
    };

    //File Attachment-----End

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.sopSubCategoryNewForm.$valid) {

            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb.';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;
            $scope.sopSubCategoryNew.FileName = fileName;
            if (!baseService.isUndefinedOrNull($scope.sopSubCategoryNew.FileName)) {
                if ($scope.sopSubCategoryNew.FileName.length > 50) {
                    throw "File Name must be less than 50 character."
                }
            }
            var formData = new FormData();

            angular.copy($scope.sopSubCategoryNew, $scope.SOPSubCategory);
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        formData.append("SOPSubCategory", angular.toJson(data.SOPSubCategory));
                        if (baseService.isUndefinedOrNull($scope.filedata) == false) {
                            formData.append('file', data.file);
                        }
                        return formData;
                    },
                    data: { 'SOPSubCategory': $scope.SOPSubCategory, 'file': $scope.filedata }
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.sopSubCategories.push(response.data.SOPSubCategory);
                        $scope.sopSubCategories = $filter('orderBy')($scope.sopSubCategories, 'Sequence');
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
                        formData.append("SOPSubCategory", angular.toJson(data.SOPSubCategory));
                        if (baseService.isUndefinedOrNull($scope.filedata) == false) {
                            formData.append('file', data.file);
                        }
                        return formData;
                    },
                    data: { 'SOPSubCategory': $scope.SOPSubCategory, 'file': $scope.filedata }
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.sopSubCategories[$scope.index] = $scope.SOPSubCategory;
                            $scope.sopSubCategories = $filter('orderBy')($scope.sopSubCategories, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.sopSubCategoryNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.sopSubCategoryNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.sopSubCategories.splice($scope.index, 1);
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
        $scope.SOPSubCategory = {};
        $scope.sopSubCategoryNew = {};
        $scope.sopSubCategoryNew.Sequence = seq;
        $scope.sopSubCategoryNew.Active = true;
    }
};