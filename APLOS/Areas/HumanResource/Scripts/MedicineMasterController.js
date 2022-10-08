'use strict';
MedicineMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function MedicineMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Medicine Master';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/MedicineMaster/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.saveUrlP = $scope.path + 'SavePurpose';
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
   $scope.GetSequence();
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

    $scope.MedicinePurposeList = [];
    $scope.userMPList = [];
    $scope.getMedicinePurpose = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getMedicinePurpose",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MedicinePurposeList = response.data;
            
        });
    }
    $scope.getMedicinePurpose();
    // ================================================GET MAIN GRID DATA CLOSE====================================================

    // ================================================FORM OBJECT DECLARATION & INITIALIZATION====================================
    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        ShortName: null,
        StandardName: null,
        UserName:null,
        Category: null,
        SubCategory: null,       
        Rate: null,
        MedicinePurposeId:null,
        Remarks: null,
        IsActive: true
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    
    // ================================================FORM OBJECT DECLARATION & INITIALIZATION=====================================

    //=======================================DOUBLE CLICK ON GRID OPEN FORM============================================
    
    //Double Clicking The PA Header Grid
    
    $scope.Get = function (args) {
        var prpseArr = args.data.MedicinePurpose.split(',');
        $("#medicinePurposeId").data("ejDropDownList").selectItemByText(prpseArr);
        $scope.ModelNew = Object.assign({}, args.data, $("#medicinePurposeId").data("ejDropDownList").selectItemByText(prpseArr));
       
        $scope.Action = 'Update';

        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
            
        }
    };
    //=======================================DOUBLE CLICK ON GRID OPEN FORM CLOSE============================================

   

    //=======================================SAVE============================================
   
    $scope.Save = function () {
        var DropDownJobLocationListObjP = $("#medicinePurposeId").data("ejDropDownList");
        var mdcnPrpsLists = DropDownJobLocationListObjP.getSelectedValue().split(",");

        if (mdcnPrpsLists.length < 1) {
            ShowResult('Medicie purpose are not selected!', 'failure');
            throw ("Invalid Request!");
        }
        $scope.$broadcast('show-errors-check-validity');

        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: {
                'data': $scope.ModelNew,
                'medicinepurpose': mdcnPrpsLists
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
            Category: null,
            SubCategory: null,
            ItemName: null,
            Rate: 0.00,
            Purpose: null,
            Remarks: null,
            IsActive: true
        };
        $("#medicinePurposeId").data("ejDropDownList").clearText();

        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }

    
    //=======================================CLEAR FORM======================================
}