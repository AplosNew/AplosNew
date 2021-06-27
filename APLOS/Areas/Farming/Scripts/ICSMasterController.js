'use strict';
ICSMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter','fileReader'];
function ICSMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, fileReader) {
    $rootScope.title = 'ICSMaster';
    $scope.ICSMasterList = [];
    $scope.ToDoFilePath = virtualPath.ICSMaster;

    $scope.path = 'Farming/ICSMaster/';

    $scope.getListUrl = $scope.path + 'getlist';

    $scope.saveUrl = $scope.path + 'create';
    
    $scope.deleteUrl = $scope.path + 'delete/';



    baseService.init($scope.getListUrl);


    $scope.searchBy = "Name"; $scope.search = "";


    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'RegistrationCode', name: "RegistrationCode" }, { value: 'Group', name: "Group" }, { value: 'Name', name: "Name" }];



    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ICSMasterList = response.data;
            ClearFields();
            ClearDocument();
        });
    }
    $scope.getData();

    $("#uploadBtn4").change(function () {
        $scope.filedata = this.files[0];
    });

    document.getElementById("uploadBtn4").onchange = function () {
        var filename = document.getElementById("uploadFile4").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile4").value = res;
    };

    //File Download

    $scope.FileDownload = function (FileName) {
        $scope.dwonloadUrl = null;
        $scope.FileName = FileName;
        var str = FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.ICSMaster + '/' + data.FileId + extention;
    };

    //Detach file  button Method and id confirmDocumentDelete
    $scope.DocumentRemove = function () {
        angular.element(document.querySelector('#confirmDocumentDelete')).modal('show');
    };
    $scope.removeDocument = function () {
        document.getElementById('uploadBtn4').value = '';
        $scope.filedata = '';
        $scope.ICSMaster.FileName = "";
        $scope.filedata = {};
        document.getElementById('uploadFile4').value = "";
        angular.element(document.querySelector('#confirmDocumentDelete')).modal('hide');
    };

    //ICSMaster Detach file method
    $scope.confirmCloseDocumentDelete = function () {
        angular.element(document.querySelector('#confirmDocumentDelete')).modal('hide');
    };
    // Clear Method for ICSMaster
    function ClearDocument() {
        document.getElementById('uploadBtn4').value = '';
        $scope.filedata = '';
        $scope.ICSMaster.FileName = "";
        $scope.filedata = {};
        document.getElementById('uploadFile4').value = "";
    };

    //File Attachment-----End

    $scope.ModelTemp = {
        Id: null,
        RegistrationCode: null,
        Group: null,
        Name: null,
        LicenseNumber: null,
        RegistrationID: null,
        RegistrationDate: null,
        RenewalPeriod: null,
        UserInfo1: null,
        UserInfo2: null,
        Remarks: null,
        ResponsiblePersonId: null,
        FileName: null,
        DebitGL: null,
        CreditGL: null,
        EmployeeStatus: null,
        EntityId: null,
        PlantId: null,
        CompanyId: null,
    };
    $scope.ICSMaster = Object.assign({}, $scope.ModelTemp);

    $scope.Get = function (args) {

        $scope.ICSMaster = Object.assign({}, args.data);
        $scope.getPlant();
        $scope.getEntityWithChange();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Action = 'Save';

    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });

    $scope.PlantList = [];
    $scope.getPlant = function () {
        cboService.getCboPlantByCompany($scope.ICSMaster.CompanyId, function (result) {
            $scope.PlantList = result;
        });
    };

    $scope.EntityList = [];
    $scope.getEntityWithChange = function () {
        $scope.EntityList = [];
        cboService.getCboEntityByPlant(null, $scope.ICSMaster.CompanyId, $scope.ICSMaster.PlantId, function (result) {
            $scope.EntityList = result;
        });
    };

    // To show data in grid
    $scope.Getgrid = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ICSMasterList = response.data;

        });
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.General.$valid) {
            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;
            $scope.ICSMaster.FileName = fileName;
            if (!baseService.isUndefinedOrNull($scope.ICSMaster.FileName)) {
                if ($scope.ICSMaster.FileName.length > 50) {
                    throw "File Name must be less than 50 character.";
                }
            }
            var formData = new FormData();
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    formData.append("ICSMaster", angular.toJson(data.ICSMaster));
                    if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                        formData.append('file', data.file);
                    }
                    return formData;
                },
                data: { 'ICSMaster': $scope.ICSMaster, 'file': $scope.filedata }

            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ICSMaster = response.data.Data;
                    //            $scope.Action = 'Update';
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

            // }
        }
    };



    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ICSMaster.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ICSMaster.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();

        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ICSMaster = Object.assign({}, $scope.ModelTemp);
        ClearDocument();
    }


    ///////////////////////////////////  Responsible Person Pop Up  ////////////////////////////////////////


    // #region ResPerson field


    $scope.EmpResPersonList = [];
    $scope.ResponsiblePersonPopUp = function () {
        angular.element(document.querySelector("#EmployeePopUpResPerson")).modal("show");
        $scope.getEmpDetailsData();

    }
    $scope.getEmpDetailsData = function () {
        $scope.EmpResPersonList = [];

        $http({
            method: 'POST',
            data: { Id: $scope.ICSMaster.Id },
            url: $scope.path + 'LoadAllEmpDetailsForSelection'
        }).then(function successCallback(response) {
            $scope.EmpResPersonList = response.data;
        });
    }

    $scope.ResponsiblePersonClear = function () {
        $scope.ICSMaster.ResponsiblePersonId = null;
        $scope.ICSMaster.ResponsiblePerson = null;
        $scope.ICSMaster.EmployeeCode = null;
        $scope.ICSMaster.EmployeeStatus = null;
    };
    $scope.closeEmpResPersonPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setEmpData = function (obj) {

        var data = obj.data;
        $scope.ICSMaster.EmployeeCode = data.Code;
        $scope.ICSMaster.ResponsiblePersonId = data.Id;
        $scope.ICSMaster.ResponsiblePerson = data.EmployeeName;
        angular.element(document.querySelector('#EmployeePopUpResPerson')).modal('hide');
    };
    // # end region ResPerson

    ///////////////////////////////////  Responsible Person Pop Up End ////////////////////////////////////////

}