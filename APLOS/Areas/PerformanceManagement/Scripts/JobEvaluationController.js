'use strict';
JobEvaluationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function JobEvaluationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Job Evaluation';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'PerformanceManagement/JobEvaluation/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "p.UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'p.UserName', name: "Position Name" }, { value: 'p.Code', name: "Position Code" }, { value: 'div.UserName', name: "Division" }, { value: 'dept.UserName', name: "Department" }];


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {          
            $scope.ModelList = response.data;
            ClearFields();
  
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        EvaluationDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        PositionCodeId: null,
        PositionCode: null,
        EvaluatorNameId: null,
        ApprovedById: null,
        PositionName: null,
        ResponsiblePerson: null,
        EmployeeCode: null,
        EmployeeStatus: null,
        EmpStatus: null,
        ApprovedByName: null,
        EmpCode: null,

    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.ModelNew.EvaluationDate = $scope.ModelNew.JobEvalDate;
        $scope.ModelNew.PositionName = $scope.ModelNew.Position;
        $scope.getJobEvalChildData();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    // To show data in grid
    $scope.Getgrid = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;

        });
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.GeneralForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ModelNew = response.data.Data;                 
                    $scope.Getgrid();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

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
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        $scope.getData();
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.getJobEvalChildData();
    }

    // #region Position field

    $scope.PositionList = [];
    $scope.PositionPopUp = function () {
        angular.element(document.querySelector("#PosPopUp")).modal("show");
        $scope.getPosDetailsData();

    }
    $scope.getPosDetailsData = function () {
        $scope.PositionList = [];
        $http({
            method: 'POST',
            data: { Id: $scope.ModelNew.Id },
            url: $scope.path + 'LoadAllPositionDetailsForSelection'
        }).then(function successCallback(response) {
            $scope.PositionList = response.data;
        });
    }

    $scope.PositionClear = function () {
        $scope.ModelNew.PositionCodeId = null;
        $scope.ModelNew.PositionName = null;
        $scope.ModelNew.PositionCode = null;
    };
    $scope.closePositionPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }

    $scope.setPositionData = function (obj) {
        var data = obj.data;
        $scope.ModelNew.PositionCode = data.Code;
        $scope.ModelNew.PositionCodeId = data.Id;
        $scope.ModelNew.PositionName = data.UserName;
        angular.element(document.querySelector('#PosPopUp')).modal('hide');
    };
    // # end region

    // Job Evaluation Child data

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.JobEvalAttributeList = [];
    $scope.SelectedJobEvalChildTabList = [];

    $http({
        method: 'GET',
        url: $scope.path+ 'getjobevalattributelist',
    }).then(function successCallback(response) {
        $scope.JobEvalAttributeList = response.data;
        });

    $scope.CheckFactoring = function () {
        try {

            if ($scope.JobEvalChild.Factoring !== null) {
                var val = parseFloat(2);
                if ($scope.JobEvalChild.Factoring > val) {
                    $scope.JobEvalChild.Factoring = parseFloat(1);
                    throw 'Factoring should be less than 2';
                }          
            }

        }
        catch (e) {
            ShowResult(e, "failure");
        }

    }

    $scope.JobEvalChildModelTemp = {
        Id: null,
        JobEvaluationId: null,
        JobEvaluationMasterId: null,
        JobEvaluationAttributeId: null,
        JobEvalMaster: null,
        Factoring: 1,
        Remarks: null,

    };
    $scope.JobEvalChild = Object.assign({}, $scope.JobEvalChildModelTemp);

    $scope.SaveJobEvalChild = function () {

        $scope.$broadcast('show-errors-check-validity');
        if ($scope.JobEvalChildForm.$valid) {

            $http({
                method: 'POST',
                url: $scope.path + 'SaveJobEvalChildData',
                data: { 'data': $scope.JobEvalChild, 'MasterId': $scope.ModelNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.JobEvalChild = response.data.CData;
                    $scope.getJobEvalChildData();
                    ClearFieldsJobEvalChildData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };


    function ClearFieldsJobEvalChildData() {

        $scope.JobEvalChild = Object.assign({}, $scope.JobEvalChildModelTemp);
    }

    $scope.getJobEvalChildData = function () {

        $http({
            method: 'GET',
            url: $scope.path + 'getJobEvalChildData?MasterId=' + $scope.ModelNew.Id
        }).then(function successCallback(response) {
            $scope.SelectedJobEvalChildTabList = response.data;
            
        });
    }


    $scope.DelJobEChild = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DelJobEChild?Id=' + $scope.JobEvalChildTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getJobEvalChildData();
                ClearFieldsJobEvalChildData();
            }

        });
    }

    $scope.ConfirmDeleteJobEvaluationChildTab = function (Id) {
        $scope.JobEvalChildTabId = Id;
        angular.element(document.querySelector("#DeleteJobEvalChildTabPopUp")).modal("show");
    }

    // #region Job Evaluation Master field

    $scope.JobEvalMstList = [];
    $scope.JobEvalPopUp = function () {
        angular.element(document.querySelector("#JobEvalMstPopUp")).modal("show");
        $scope.getJobEvalDetails();

    }
    $scope.getJobEvalDetails = function () {
        $scope.JobEvalMstList = [];
        $scope.DimShow = false;
        $scope.CatShow = true;
        $http({
            method: 'POST',
            data: { MasterId: $scope.ModelNew.Id, JobEvalAttributeId: $scope.JobEvalChild.JobEvaluationAttributeId },
            url: $scope.path + 'LoadAllJobEvalDetailsForSelection'
        }).then(function successCallback(response) {
            $scope.JobEvalMstList = response.data;
            if (baseService.arrayLength($scope.JobEvalMstList) > 0) {       
                if ($scope.JobEvalMstList[0].DimensionApp == "Yes") {
                    $scope.DimShow = true;
                    $scope.CatShow = false;
                }
                if ($scope.JobEvalMstList[0].DimensionApp == "No") {
                    $scope.DimShow = false;
                    $scope.CatShow = true;
                }
            }
        });
    }

    $scope.closeJobEvalMstPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setJobEvalMstData = function (obj) {

        var data = obj.data;
        $scope.JobEvalChild.JobEvaluationMasterId = data.Id;
        $scope.JobEvalChild.JobEvalMaster = data.AttributeStandardName;
        angular.element(document.querySelector('#JobEvalMstPopUp')).modal('hide');
    };
    // # end region

    // #region field

    $scope.EmpResPersonList = [];
    $scope.ResponsiblePersonPopUp = function () {
        angular.element(document.querySelector("#EmployeePopUpResPerson")).modal("show");
        $scope.getEmpDetailsData();

    }
    $scope.getEmpDetailsData = function () {
        $scope.EmpResPersonList = [];
        $http({
            method: 'POST',
            data: { Id: $scope.ModelNew.Id },
            url: $scope.path + 'LoadAllEvaluatorDetails'
        }).then(function successCallback(response) {
            $scope.EmpResPersonList = response.data;
        });
    }

    $scope.ResponsiblePersonClear = function () {
        $scope.ModelNew.EvaluatorNameId = null;
        $scope.ModelNew.ResponsiblePerson = null;
        $scope.ModelNew.EmployeeCode = null;
        $scope.ModelNew.EmployeeStatus = null;

    };
    $scope.closeEmpResPersonPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setEmpData = function (obj) {

        var data = obj.data;
        $scope.ModelNew.EmployeeCode = data.Code;
        $scope.ModelNew.EvaluatorNameId = data.Id;
        $scope.ModelNew.ResponsiblePerson = data.EmployeeName;
        angular.element(document.querySelector('#EmployeePopUpResPerson')).modal('hide');
    };
    // # end region

    // #region field

    $scope.ApprovedByList = [];
    $scope.ApprovedByPopUp = function () {
        angular.element(document.querySelector("#ApprovedPopUp")).modal("show");
        $scope.getapprovedbyData();

    }
    $scope.getapprovedbyData = function () {
        $scope.ApprovedByList = [];

        $http({
            method: 'POST',
            data: { Id: $scope.ModelNew.Id },
            url: $scope.path + 'LoadApprovedbyDetails'
        }).then(function successCallback(response) {
            $scope.ApprovedByList = response.data;
        });
    }

    $scope.ApprovedByClear = function () {
        $scope.ModelNew.ApprovedById = null;
        $scope.ModelNew.ApprovedByName = null;
        $scope.ModelNew.EmpCode = null;
        $scope.ModelNew.EmpStatus = null;
    };
    $scope.closeapprovedbyPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setApprovedByData = function (obj) {

        var data = obj.data;
        $scope.ModelNew.EmpCode = data.Code;
        $scope.ModelNew.ApprovedById = data.Id;
        $scope.ModelNew.ApprovedByName = data.EmployeeName;
        angular.element(document.querySelector('#ApprovedPopUp')).modal('hide');
    };
    // # end region


    //********** Tab end ***************
}