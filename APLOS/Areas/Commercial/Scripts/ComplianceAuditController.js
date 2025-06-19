'use strict';
ComplianceAuditController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ComplianceAuditController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Compliance Audit';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.ComplianceList = [];
    $scope.path = 'Commercial/Compliance/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'CreateAudit';
    $scope.deleteUrl = $scope.path + 'DeleteTransaction/';
    $scope.Action = 'Save';
    $scope.searchBy = "Code"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'Remarks', name: "Remarks" }];

    $scope.Grouplist = [];
    $scope.GetGroupCbo = function () {
        $http.get('Commercial/Compliance/GetGroupCbo')
            .then(function (response) {
                $scope.Grouplist = response.data;
            });
    };
    $scope.GetGroupCbo();

    $scope.CategoryList = [];
    $scope.GetCategoryCbo = function () {
        $http.get('Commercial/Compliance/GetCategoryCbo')
            .then(function (response) {
                $scope.CategoryList = response.data;
            });
    };
    $scope.GetCategoryCbo();

    $scope.SubCategoryList = [];
    $scope.GetSubCategoryCbo = function () {
        $http.get('Commercial/Compliance/GetSubCategoryCbo')
            .then(function (response) {
                $scope.SubCategoryList = response.data;
            });
    };
    $scope.GetSubCategoryCbo();

    $scope.getComplianceData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetComplianceDataList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ComplianceList = response.data;
        });
    }
    $scope.getComplianceData();

    $scope.CriticalityLevelList = [
        { 'Value': "Normal", 'Text': "Normal" },
        { 'Value': "Critical", 'Text': "Critical" },
        { 'Value': "Important", 'Text': "Important" }
    ];

    $scope.auditFrequencyUnitList = [
        { 'Value': "Days", 'Text': "Days" },
        { 'Value': "Hour", 'Text': "Hour" }
    ];

    $scope.ComplianceValueList = [
        { 'Value': "0", 'Text': "0" },
        { 'Value': "1", 'Text': "1" },
        { 'Value': "2", 'Text': "2" },
        { 'Value': "3", 'Text': "3" },
        { 'Value': "4", 'Text': "4" }
    ];


    $scope.ScorePointList = [
        { 'Value': "1", 'Text': "1" },
        { 'Value': "2", 'Text': "2" },
        { 'Value': "3", 'Text': "3" },
        { 'Value': "4", 'Text': "4" }
    ];


    $scope.CheckMarkList = [
        { 'Value': "True", 'Text': "Yes" },
        { 'Value': "False", 'Text': "No" }
    ];

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.ModelNew.ComplianceMasterId = $scope.ModelNew.Id;
        $scope.ModelNew.ComplianceValue = $scope.ModelNew.ComplianceValue.toString();
        if ($scope.ModelNew.RPACount == 2) {
            $("#CreateNewPopUp").data("ejDialog").open();
        }
        else {
            $scope.GetComplianceAuditDataList();
            $scope.closePopup('CreateNewPopUp');
            $scope.Action = 'Update';
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }

    };

    $scope.SourceType = "";

    $scope.Go = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.SourceType)) {
                throw "Select Responsible Person or Auditor.";
            }
            $scope.GetComplianceAuditDataList();
           
            $scope.closePopup('CreateNewPopUp');
            $scope.Action = 'Update';
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.closePopup = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");
        try {
            $("#" + popupName).data("ejDialog").close();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.openPopup = function (popupName) {

        try {
            $("#" + popupName).data("ejDialog").open();
        } catch (e) {

        }
    }

    $scope.GetComplianceAuditDataList = function () {
        $http.get('Commercial/Compliance/GetComplianceAuditDataList?masterId=' + $scope.ModelNew.ComplianceMasterId)
            .then(function (response) {
                if (response.data.length > 0) {
                    $scope.ModelNew = Object.assign({}, response.data[0]);
                    $scope.ModelNew.ComplianceValue = $scope.ModelNew.ComplianceValue.toString();
                }
                $scope.GetCheckPointsList();
            });
    }

    $scope.CheckPList = [];
    $scope.GetCheckPointsList = function () {
        $scope.CheckPList = [];
        $http.get('Commercial/Compliance/GetComplianceCheckPointsData?masterId=' + $scope.ModelNew.ComplianceMasterId)
            .then(function (response) {
                $scope.CheckPList = response.data;
            });
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
           
            $scope.ModelNew.EmpSystemId = null;
            $scope.CheckList = [];
            for (var i = 0; i < $scope.CheckPList.length; i++) {
                var ob = {};
                ob.Id = $scope.CheckPList[i].Id == null ? Math.floor(Math.random() * 9) - 10 : $scope.CheckPList[i].Id;
                ob.CheckPointsId = $scope.CheckPList[i].CheckPointsId;
                ob.CheckMark = $scope.CheckPList[i].CheckMark;
                $scope.CheckList.push(ob);
                ob = {};
            }

            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew, 'CheckPList': $scope.CheckList, 'SourceType': $scope.SourceType },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getCTData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Clear = function () {
        $scope.ModelNew = {};
        $scope.CheckPList = [];
        $scope.CheckList = [];
        
        
    }


}