'use strict';
jwLocationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', '$window'];
function jwLocationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, $window) {
    $rootScope.title = 'Job Work Location';
    $scope.ModelList = [];
    $scope.path = 'Outsourcing/JWLocation/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";

    $scope.companyId = "";//$window.companyId;

    $scope.plantId = "";//$window.plantId;

    $scope.entityList = [];
    $scope.cboPlantList = [];
    $scope.serviceCboList = [];
    $scope.storageLocationList = [];
    $scope.storageLocationListAll = [];

    $scope.cboCompanyList = [];

    cboService.getCboCompanyByCompanyGroup($window.companyGroupId, function (result) {
        $scope.cboCompanyList = result;
    });

    //$scope.GetPlantList = function () {
    //    $scope.cboPlantList = ej.DataManager($scope.plantListA).executeLocal(ej.Query().where("CompanyId", "equal", $scope.companyId));
    //};
    $scope.GetPlantList = function () {
        $scope.cboPlantList = ej.DataManager($scope.plantListA).executeLocal(ej.Query().where("CompanyId", "equal", $scope.companyId));
    };


    // $scope.GetPlantList();
    $scope.getEntityList = function () {

        $scope.entityList = ej.DataManager($scope.entityListA).executeLocal(ej.Query().where("PlantId", "equal", $scope.plantId));
    };


    $scope.entityListA = [];
    $scope.getEntityListA = function () {
        $http.get('Outsourcing/JWLocation/GetEntityListA')
            .then(function (response) {
                $scope.entityListA = response.data;
                $scope.entityList = response.data;

            });
    };
    $scope.getEntityListA();
    $scope.plantListA = [];
    $scope.getPlantListA = function () {
        $http.get('Outsourcing/JWLocation/GetPlantList')
            .then(function (response) {
                $scope.plantListA = response.data;
                $scope.cboPlantList = response.data;
            });
    };
    $scope.getPlantListA();


    $http.get('Outsourcing/JWLocation/GetStorageLocationList')
        .then(function (response) {
            $scope.storageLocationListAll = response.data;
            $scope.storageLocationList = response.data;
        });
    $scope.GetStorageLocation = function () {
        $scope.storageLocationList = ej.DataManager($scope.storageLocationListAll).executeLocal(ej.Query().where("PlantId", "equal", $scope.plantId));
    };

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    };
    $scope.getData();
    $scope.JWActivityList = [];

    $scope.GetJWActivityList = function (activityId) {
        $http({
            method: 'POST',
            url: $scope.path + "GetJWActivityList",
            dataType: 'JSON'
        }).then(function successCallback(response) {


            $scope.JWActivityList = response.data;
        });
    };
    $scope.GetJWActivityList();

    $scope.JWActivityList = [];

    $scope.GetJWLocationActivityListById = function (jwLocationId) {
        $http({
            method: 'POST',
            url: $scope.path + "GetJWLocationActivityListById",
            data: { 'jwLocationId': jwLocationId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.JWActivityListTemp = response.data;
            // $("#GridJWActivity").ejGrid("instance");

        });
    };

    //#region Partial View
    $controller("employeeBaseController", { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.materialType = ['Asset', 'Consumable', 'Spare', 'RawMaterial'];
    //#endregion

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        EntityId: null,
        ResponsiblePersonId: null,
        Remarks: null,
        StorageLocationId: null,
        ServiceId: null,
        ResponsiblePersonName: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.selectResponsiblePersonPopUp = function (index, id) {
        $scope.updateResponsiblePersonIndex = index;
        $scope.selectedResponsiblePerson = id;
    };
    $scope.updateResponsiblePersonIndex = -1;
    $scope.closeResponsiblePersonPopUp = function () {
        //if ($scope.updateResponsiblePersonIndex !== -1) {
        //    var employee = $scope.employeeList[$scope.updateResponsiblePersonIndex];
        //    $scope.ModelNew.ResponsiblePersonName = employee.EmployeeName;
        //    $scope.ModelNew.ResponsiblePersonId = employee.SystemId;
        //}
        angular.element(document.querySelector("#responsiblePersonPopUp")).modal("hide");
    };
    $scope.clearResponsiblePerson = function () {
        $scope.ModelNew.ResponsiblePersonName = null;
        $scope.ModelNew.ResponsiblePersonId = null;
    };

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.companyId = args.data.CompanyId;
        $scope.plantId = args.data.PlantId;
        $scope.cboPlantList = $scope.plantListA;
        $scope.entityList = $scope.entityListA;
        $scope.storageLocationList = $scope.storageLocationListAll;

        $scope.GetJWLocationActivityListById(args.data.Id);
    };

    $scope.Save = function () {

        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew, 'ActivityList': $scope.JWActivityListTemp },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //ClearFields(response.data.Sequence);
                    $scope.Action = 'Update';
                    $scope.ModelNew = response.data.Data;
                    $scope.getData();
                    // $scope.GetJWLocationActivityListById();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

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
                    ClearFields(response.data.Sequence);
                    Clear
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }

            });
        }
    };

    $scope.JWActivityList = [];
    $scope.JWActivityListTemp = [];

    $scope.GetJWActivityList = function (activityId) {

        var parameters = { 'activityId': activityId };
        $http({
            method: "POST",
            dataType: 'JSON',
            url: $scope.path + "GetJWActivityList",
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.empGrid = true;

                $scope.JWActivityList = response.data;

            }
            //var gridObj = $("#empInfoGrid").data("ejGrid");

        });

    };
    $scope.GetJWActivityList(null);


    function checkChangeJWActivity(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.JWActivityList, { 'Id': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState == "check")
                row[0].Active = true;
            else
                row[0].Active = false;
        }

    }
    function headcheckChangeJWActivity(e) {
        if (e.model.checkState == "check") {

            // var gridObj = $("#GridJWActivity").data("ejGrid");
            var filtered = $("#GridJWActivity").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.JWActivityList.length; i++) {

                    $scope.JWActivityList[i].isSelect = true;
                }
            }
            else {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.JWActivityList[i].EmpSystemId == filtered[j].EmpSystemId)
                            // $scope.EmployeeList[i].isSelect = true;
                            $scope.JWActivityList[i].isToBeSelect = true;
                    }

                }
            }

            var checkbox = $("#GridJWActivity .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridJWActivity .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridJWActivity .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#GridJWActivity .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeJWActivity });
            }
        }
        else {
            var filtered = $("#GridJWActivity").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.JWActivityList.length; i++) {
                    $scope.JWActivityList[i].isToBeSelect = false;
                }
            }
            else {
                for (var i = 0; i < $scope.JWActivityList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.JWActivityList[i].Id == filtered[j].Id)
                            $scope.JWActivityList[i].isToBeSelect = false;
                    }

                }
            }
            var checkbox = $("#GridJWActivity .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridJWActivity .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridJWActivity .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#GridJWActivity .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeJWActivity });
            }
        }
        //header level check
    }
    $scope.dataBoundemployee = function (args) {
        $("#GridJWActivity .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk").ejCheckBox({ "change": headcheckChangeJWActivity });

    };
    $scope.refreshTemplateJWActivity = function (args) {
        if (args.rowIndex == 0) {
            $("#headchk").ejCheckBox({ "change": headcheckChangeJWActivity });
        }

        var valobj = $($("#GridJWActivity .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#GridJWActivity .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#GridJWActivity .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.EmployeeList, { 'EmpSystemId': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].isToBeSelect == true)
                $($("#GridJWActivity .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#GridJWActivity .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#GridJWActivity .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeJWActivity });
    };
    $scope.saveJWActivitydata = function () {
        $scope.JWActivityListTemp = [];
        var row = $filter('filter')($scope.JWActivityList, { 'isToBeSelect': true });

        $scope.JWActivityListTemp = row;


        $scope.Back();
    };
    //$scope.showJWActivityFilterScreen = function () {
    //    try {

    //      var gridObj = $("#GridJWActivity").data("ejGrid");
    //   gridObj.clearFiltering();
    //      angular.element(document.querySelector('#empfilterPopUp')).modal('show');


    // } catch (e) {
    //    ShowResult(e, 'failure');
    // }
    // };

    $scope.addactivity = function () {
        try {

            for (var i = 0; i < $scope.JWActivityList.length; i++) {
                $scope.JWActivityList[i]["isToBeSelect"] = false;

                for (var k = 0; k < $scope.JWActivityListTemp.length; k++) {
                    if ($scope.JWActivityList[i]["JWActivityId"] == $scope.JWActivityListTemp[k]["JWActivityId"]) {
                        $scope.JWActivityList[i]["isToBeSelect"] = true;
                    }


                }
            }

            angular.element(document.querySelector('#JWActivityFilterPopUp')).modal('show');


        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.Back = function () {
        angular.element(document.querySelector('#JWActivityFilterPopUp')).modal('hide');
    };



    $scope.employeeList = [];
    $scope.showAllEmployeeListPopUp = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Outsourcing/JWActivity/EmployeeListAll'
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.employeeList = response.data;
                angular.element(document.querySelector('#responsiblePersonPopUp')).modal('show');
            }
            else {
                ShowResult("No Data Found", 'failure');
            }
        });
    };
    $scope.getEmp = function (obj) {
        $scope.ModelNew.ResponsiblePersonId = obj.data.SystemId;
        $scope.ModelNew.ResponsiblePersonName = obj.data.EmployeeName;
        angular.element(document.querySelector('#responsiblePersonPopUp')).modal('hide');
    };


    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
        $scope.JWActivityListTemp = [];
        $scope.companyId = null;
        $scope.storageLocationList = [];
        $scope.cboPlantList = [];
        $scope.entityList = [];

        $scope.plantId = null;
    }
}