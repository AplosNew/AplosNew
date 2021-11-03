'use strict';
employeeJobDescriptionController.$inject = ['fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window', 'toaster'];
function employeeJobDescriptionController(fileReader, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window, toaster) {
    $rootScope.title = 'Employee Job Description Category';
    $scope.index = -1;
    $scope.employeeJobDescriptions = [];
    $scope.path = 'employees/employeejobdescription/';
    $scope.getListUrl = $scope.path + 'getjobdescriptionlist';
    $scope.isExist = false;

    // #region Toaster
    $scope.popCode = function (type, msg) {
        toaster.pop({
            type: type,
            body: msg,
            timeout: 5000
        });
    };
    // #endregion
    $scope.Code = null;
    $scope.Position = null;
    $scope.Entity = null;
    $scope.message_confirmation = null;
    $scope.getemployeeJobDescriptions = function () {
        try {
            if (baseService.isUndefinedOrNull($window.employeeId)) {
                $scope.isExist = false;
                throw "JD is aplicable only for Employee";
            }
            else {

                $http({
                    method: 'GET',
                    url: 'employees/employeejobdescription/getjobdescriptionlist'//?employeeId=' + $window.employeeId
                }).then(function (response) {
                    $scope.employeeJobDescriptions = response.data;
                    $scope.Code = response.data[0].Code;
                    $scope.Position = response.data[0].Position;
                    $scope.Entity = response.data[0].Entity;
                    $scope.isExist = true;
                    if (baseService.arrayLength($scope.employeeJobDescriptions) === 0) {
                        $scope.message_confirmation = 'No Job Description found in SOP.';
                        $scope.isExist = false;
                        angular.element(document.querySelector('#JDPopUp')).modal('show');
                    }
                });
            }//else
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.getemployeeJobDescriptions();

    $scope.typeflag = '';
    $scope.showStat = function (args, flag) {
        $scope.SelectedData = args.data;
        $scope.SelectedDataFields = Object.assign({}, $scope.SelectedDataFieldsTemp);

        $scope.typeflag = flag;
        if (flag == 'SOPDocument') {
            $scope.GetActivityDocumentList($scope.SelectedData.Id);
        }
        if (flag == 'SOPItemDocument') {
            $scope.GetSOPDocumentList($scope.SelectedData.SOPItemId);
        }
    }

    $scope.ActivityDocumentList = [];
    $scope.GetActivityDocumentList = function (SOPActivityId) {
        $http({
            method: 'GET',
            url: 'employees/employeejobdescription/GetActivityDocumentList?SOPActivityId=' + SOPActivityId
        }).then(function (response) {
            $scope.ActivityDocumentList = response.data;
        });
        angular.element(document.querySelector('#ACTDOCPopUp')).modal('show');
    };

    $scope.SOPDocumentList = [];
    $scope.GetSOPDocumentList = function (SOPItemId) {
        $http({
            method: 'GET',
            url: 'employees/employeejobdescription/GetSOPDocumentList?SOPItemId=' + SOPItemId
        }).then(function (response) {
            $scope.SOPDocumentList = response.data;
        });
        angular.element(document.querySelector('#SOPDOCPopUp')).modal('show');
    };


    $scope.getfileModalData = function (JdId) {
        angular.element(document.querySelector('#filePopUp')).modal('show');
        $http({
            method: 'GET',
            url: 'employees/employeejobdescription/getfilelist?jdId=' + JdId
        }).then(function (response) {
            $scope.fileList = response.data;
        });
    };

   
    $scope.FileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.SOPActivityDocument + '/' + data.FileId + extention;
    };

    $scope.SOPActivityDocFileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.SOPDocument + '/' + data.FileId + extention;
        
    };
    
}