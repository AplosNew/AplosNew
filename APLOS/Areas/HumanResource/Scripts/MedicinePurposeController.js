'use strict';
MedicinePurposeController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function MedicinePurposeController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Medicine Purpose';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/MedicinePurpose/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
   
    $scope.deleteUrl = $scope.path + 'Delete/';
    baseService.init($scope.getListUrl);
    $scope.downloadgriddataUrl = 'GridReports/Download';

    // ================================================SEQUENCE====================================================
    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    //$scope.GetSequence();
    // ================================================SEQUENCE CLOSE====================================================

    // ================================================GET MAIN GRID DATA====================================================

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetSequence();
        });
    }
    $scope.getData();

    $scope.CategoryList = [];
    $scope.getCategory = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetCategory",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.CategoryList = response.data;
            
        });
    }
    $scope.getCategory();
    // ================================================GET MAIN GRID DATA CLOSE====================================================

    // ================================================FORM OBJECT DECLARATION & INITIALIZATION====================================
    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        ShortName: null,
        StandardName: null,
        UserName: null,      
        Purpose: null,
        MedicineCategoryId:null,
        Remarks: null,
        IsActive: true
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

   
    // ================================================FORM OBJECT DECLARATION & INITIALIZATION=====================================

    //=======================================DOUBLE CLICK ON GRID OPEN FORM============================================
    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.EmployeeId = args.data.ResponsiblePerson;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
            $scope.getResponsiblePersonId();
        }
    };
    //=======================================DOUBLE CLICK ON GRID OPEN FORM CLOSE============================================

    //=======================================SAVE============================================
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');

        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: {
                'data': $scope.ModelNew,
            },
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
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.SaveP = function () {
        $scope.$broadcast('show-errors-check-validity');

        $http({
            method: 'POST',
            url: $scope.saveUrlP,
            data: {
                'data': $scope.ModelNewP,
                'medicineMasterId': $scope.ModelNew.Id
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                ClearFieldsP();


            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };
    //=======================================SAVE CLOSE==========================================

    //=======================================DELETE FUNCTION======================================
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
    //=======================================DELETE CLOSE======================================

    //=======================================CLEAR FORM======================================
    $scope.Clear = function () {
        ClearFields();
        return true;
    };

   
    function ClearFields() {
        $scope.Action = 'Save';


        $scope.ModelTemp = {
            Id: null,
            Sequence: 0,
            ShortName: null,
            StandardName: null,
            UserName: null,
            Purpose: null,
            Category: null,
            Remarks: null,
            IsActive: true
        };

        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }

   
    //=======================================CLEAR FORM======================================
}