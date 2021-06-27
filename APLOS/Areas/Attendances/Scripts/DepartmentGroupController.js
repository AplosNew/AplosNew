'use strict';
DepartmentGroupController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function DepartmentGroupController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Department Group';
    $scope.Action = 'Save';
    $scope.path = 'Attendances/DepartmentGroup/';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';

    $scope.DepartmentListNew = [];
    $scope.DepartmentGroupModel = {
        Id: null,
        Name: null,
        Sequence: 0,
        ShortName: null,
        UserName: null,
        Description: null,
        Code: null,
        StandardName: null,
        Remarks: null,
        Active: false
    };
    
    $scope.Save = function () {
        try {
            ValidationMaster();

            var DepartmentIdList = [];
            for (var i = 0; i < $scope.DepartmentListNew.length; i++) {
                //if ($scope.DepartmentListNew[i].Id == true) {
                    DepartmentIdList.push($scope.DepartmentListNew[i]);
                //}
            }
            
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'master': $scope.DepartmentGroupModel, 'DepartmentIdList':DepartmentIdList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetDepartmenthkp();
                    $scope.DepartmentGroupModel = {};
                    $scope.DepartmentListNew = [];
                 
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    
    //-------------------------------------Common
    $scope.Clear = function (obj) {
        ClearFields($scope.GetSequence());
       
    };
    function ClearFields(obj) {
        for (var i in obj) {
            obj[i] = null;
        }
        
        $scope.DepartmentGroupModel = {};
    }

    $scope.DepartmentList = [];
    $scope.GetDepartmentInformation = function () {
        try {
            var eDialog = $("#dialogDepartmentInfo").data("ejDialog");
            eDialog.open();
            //$scope.DepartmentList = [];
            $http({
                method: 'GET',
                url: 'Attendances/DepartmentGroup/GetDepartmentInformation'
            }).then(function successCallback(response) {               
                $scope.DepartmentList = response.data.DepartmentInfo;
                });
            
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    
    $scope.GetDepartmentInformationEdit = function () {
        try {          
            $http({
                method: 'GET',
                url: 'Attendances/DepartmentGroup/GetDepartmentInformationEdit?Id=' + $scope.DepartmentGroupModel.Id
            }).then(function successCallback(response) {
                $scope.DepartmentListNew = [];
                $scope.DepartmentListNew = response.data.DepartmentInfoedit;
            });

        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    
    $scope.DepartmenthkpList = [];
    $scope.GetDepartmenthkp = function () {
        try {
            $http({
                method: 'GET',
                url: 'Attendances/DepartmentGroup/GetDepartmenthkp'
            }).then(function successCallback(response) {
                $scope.DepartmenthkpList = response.data.DepartmenthkpInfo;
            });

        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.GetDepartmenthkp ();


    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#empInfoGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.DepartmentList.length; i++) {
                $scope.DepartmentList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#empInfoGrid").data("ejGrid");
        gridObj.refreshContent();
    };

    function checkDoubleDepartmentInformation(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.remove = function (obj) {
        var gridObj = $("#GridNew").data("ejGrid");
        for (var i = 0; i < $scope.DepartmentListNew.length; i++) {
            if ($scope.DepartmentListNew[i].Id === obj.data.Id) {
                $scope.DepartmentListNew.splice(i, 1);
                break;
            }
        }
        gridObj.refreshContent();
    };


    $scope.DepartmentListNew = [];
    $scope.OK = function () {
        try {
            for (var i = 0; i < $scope.DepartmentList.length; i++) {
                if ($scope.DepartmentList[i].CheckBoxSelect == true) {
                    if (checkDoubleDepartmentInformation($scope.DepartmentListNew, $scope.DepartmentList[i].Id) === false) {
                        $scope.DepartmentListNew.push($scope.DepartmentList[i]);
                    }
                }
            }
            var gridObj = $("#GridNew").data("ejGrid");
            gridObj.refreshContent();

            var eDialog = $("#dialogDepartmentInfo").data("ejDialog");
            eDialog.close();

            if ($rootScope.isCollapsed) {
                $rootScope.toggle();
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    
    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] can not be blank...";
            }

        } catch (ex) {
            throw ex;
        }
    }
    
    function ValidationMaster() {
        try { 
            CheckField("Sequence", $scope.DepartmentGroupModel.Sequence);
            CheckField("ShortName", $scope.DepartmentGroupModel.ShortName);
            CheckField("UserName", $scope.DepartmentGroupModel.UserName);
            
            CheckField("Code", $scope.DepartmentGroupModel.Code);
            CheckField("StandardName", $scope.DepartmentGroupModel.StandardName);
           
        } catch (ex) {
            throw ex;
        }
    }

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.DepartmentGroupModel.Sequence = response.data[0].Sequence;
            });
    };
    $scope.GetSequence();

    $scope.recorddoubleclick = function () {
        var gridObj = $("#GridDesignation").data("ejGrid");
        $scope.DepartmentGroupModel = gridObj.getSelectedRecords()[0];
        try {
            $scope.Action = 'Update';
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
            $scope.GetDepartmentInformationEdit();
        } catch (e) {

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.DepartmentGroupModel.Id)) {

            $http.get('Attendances/DepartmentGroup/Delete?Id=' + $scope.DepartmentGroupModel.Id)

                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetDepartmenthkp();
                        $scope.DepartmentGroupModel = {};
                        ClearFields();                       
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };
    
};