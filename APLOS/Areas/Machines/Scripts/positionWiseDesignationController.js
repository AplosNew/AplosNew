'use strict';
positionWiseDesignationController.$inject = ["cboService","commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function positionWiseDesignationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "PositionWiseDesignation";
    $scope.SkillCategoryList = [];
    $scope.CostReviewCategoryList = [];
    $scope.Action = 'Save';
    $scope.path = 'Machines/PositionWiseDesignation/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveUrlDesignationGroup = $scope.path + 'createDesignationGroup';
    $scope.saveUrlDesignation = $scope.path + 'createPositionWiseDesignation';
    $scope.exportgriddataUrlUpd = 'GridReports/ExcelExportUpd';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.SkillCategoryList = [
        {
            'Value': 'A',
            'Text': 'A'
        },
        {
            'Value': 'B',
            'Text': 'B'
        }
        ,
        {
            'Value': 'C',
            'Text': 'C'
        }
        ,
        {
            'Value': 'D',
            'Text': 'D'
        }
    ];

    $scope.CostReviewCategoryList = [
        {
            'Value': 'A',
            'Text': 'A'
        },
        {
            'Value': 'B',
            'Text': 'B'
        }
        ,
        {
            'Value': 'C',
            'Text': 'C'
        }
        ,
        {
            'Value': 'D',
            'Text': 'D'
        }
        ,
        {
            'Value': 'E',
            'Text': 'E'
        }
    ];
    
    $scope.EmployeeCategoryList = [];
    $scope.GetEmployeeCategoryList = function (pid) {
        $http({
            method: 'GET',
            url: 'Machines/PositionWiseDesignation/GetEmployeeCategoryList'
        }).then(function successCallback(response) {
            $scope.EmployeeCategoryList = response.data;
        });
    }
    $scope.GetEmployeeCategoryList();
    
    $scope.position = {
        Id: null
        , PositionCodeId: null
        , PositionCode: null
        , ResponsiblePersonId: null
        , ResponsiblePerson: null
        , PositionLevels: null
        , EmployeeCategoryId: null
        , SkillCategory: null
        , CostReviewCategory: null
        , Remarks: null
        , IsActive: true
    };
    $scope.positionNew = Object.assign({}, $scope.position);

    $scope.PositionWiseDesignationMasterList = [];
    $scope.LoadPositionWiseDesignationList = function () {
        $http({

            method: 'Get',
            url: 'Machines/PositionWiseDesignation/LoadPositionWiseDesignationList'
        }).then(function successCallback(response) {
            $scope.PositionWiseDesignationMasterList = response.data;
            var gridObj = $("#GridPositionWiseDesignationMaster").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }
    $scope.LoadPositionWiseDesignationList();

    $scope.selectPositionCode = function () {
        $scope.getPositionCode();
        angular.element(document.querySelector('#PositionCodePopUp')).modal('show');
    }

    $scope.PositionCodeList = [];
    $scope.getPositionCode = function () {
        $http({
            method: 'Get',
            url: $scope.path + 'GetPositionCode?EmployeeCategoryid=' + $scope.positionNew.EmployeeCategoryId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.PositionCodeList = resp.data;
        });
    }

    $scope.doublePositionCode = function (e) {
        $scope.positionNew.PositionCodeId = e.data.Id;
        $scope.positionNew.PositionCode = e.data.Code;
        angular.element(document.querySelector('#PositionCodePopUp')).modal('hide');
    }

    $scope.closePositionCodePopUp = function () {
        angular.element(document.querySelector('#PositionCodePopUp')).modal('hide');
    }

    $scope.selectResponsiblePerson = function () {
        $scope.getEmployee();
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('show');
    }

    $scope.EmployeeList = [];
    $scope.getEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EmployeeList = resp.data;
        });
    }

    $scope.doubleEmployee = function (e) {
        $scope.positionNew.ResponsiblePersonId = e.data.SystemId;
        $scope.positionNew.ResponsiblePerson = e.data.EmployeeName;
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }

    $scope.closeResponsiblePersonPopUp = function () {
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }

     $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
         if ($scope.PositionWiseDesignationForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'PositionData': $scope.positionNew},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadPositionWiseDesignationList();
                    PositionClearFields();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }    
    };
    $scope.Clear = function () {
        PositionClearFields();
    };

    function PositionClearFields() {
        $scope.Action = "Save";
        $scope.positionNew = Object.assign({}, $scope.position);
    }

    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: 'Machines/PositionWiseDesignation/PositionDelete?id=' + $scope.positionNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadPositionWiseDesignationList();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.GetDetails = function (args) {
        $scope.PositionMasterId = args.data.Id;
        $scope.EmpCategoryId = args.data.EmployeeCategoryId;
        $http({
            method: 'Get',
            url: 'Machines/PositionWiseDesignation/LoadPositionEditData?PositionID=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.positionNew = response.data.position[0];
            $scope.LoadDesignationGroupDetails($scope.PositionMasterId, $scope.EmpCategoryId);
            $scope.LoadPositionWiseDesignationDetails($scope.PositionMasterId);
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.PositionDesignationGroupList = [];
    $scope.LoadDesignationGroupDetails = function (pid,ecid) {
        $http({

            method: 'Get',
            url: 'Machines/PositionWiseDesignation/LoadDesignationGroupDetails?PositionId=' + pid + '&EmpCategoryId=' + ecid
        }).then(function successCallback(response) {
            $scope.PositionDesignationGroupList = response.data;
        }
        )
    }

    $scope.refreshTemplateDesignationGroup = function (args) {
        $("#DGheadchk").ejCheckBox({ "change": CheckBoxSelectAllDesignationGroup });
    };
    function CheckBoxSelectAllDesignationGroup(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridDesignationGroup").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.PositionDesignationGroupList.length; i++) {
                $scope.PositionDesignationGroupList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridDesignationGroup").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.DesignationGroupSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.PositionDesignationGroupList.length; i++) {
                if ($scope.PositionDesignationGroupList[i].Flag == true) {
                    $scope.PositionDesignationGroupList[i].PDID = $scope.positionNew.Id;
                    $scope.SaveList.push($scope.PositionDesignationGroupList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlDesignationGroup,
                data: {
                    "DataList": $scope.SaveList,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadDesignationGroupDetails($scope.positionNew.Id,$scope.positionNew.EmployeeCategoryId);
                    $scope.LoadPositionWiseDesignationDetails($scope.positionNew.Id);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.refreshTemplateDesignation = function (args) {
        $("#Dheadchk").ejCheckBox({ "change": CheckBoxSelectAllDesignation });
    };
    function CheckBoxSelectAllDesignation(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridDesignation").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.PositionWiseDesignationList.length; i++) {
                $scope.PositionWiseDesignationList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridDesignation").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.PositionWiseDesignationList = [];
    $scope.LoadPositionWiseDesignationDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'Machines/PositionWiseDesignation/LoadPositionWiseDesignationDetails?PositionID=' + pid
        }).then(function successCallback(response) {
            $scope.PositionWiseDesignationList = response.data;
        }
        )
    }

    $scope.PositionWiseDesignationReportList = [];
    $scope.LoadPositionWiseDesignationReport = function () {
        $http({

            method: 'Get',
            url: 'Machines/PositionWiseDesignation/LoadPositionWiseDesignationReports'
        }).then(function successCallback(response) {
            $scope.PositionWiseDesignationReportList = response.data;
        }
        )
    }
    $scope.LoadPositionWiseDesignationReport();

    $scope.DesignationSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.PositionWiseDesignationList.length; i++) {
                if ($scope.PositionWiseDesignationList[i].Flag == true) {
                    $scope.PositionWiseDesignationList[i].PDID = $scope.positionNew.Id;
                    $scope.SaveList.push($scope.PositionWiseDesignationList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlDesignation,
                data: {
                    "DataList": $scope.SaveList,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadPositionWiseDesignationDetails($scope.positionNew.Id);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.PositionWiseDesignationReport = function () {
        var dataList = [];
        var g = $("#GridPositionWiseDesignationReport").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.PositionWiseDesignationReportList;
        }

        $scope.fileName = "Position Wise Designation Report";

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrlUpd,
            data: { 'reportFileName': $scope.fileName, 'data': dataList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;


    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tabteam = 1;
    $scope.setTabTeam = function (newTab) {
        $scope.tabteam = newTab;
    };

    $scope.isSetteam = function (tabNum) {
        return $scope.tabteam === tabNum;
    };
}