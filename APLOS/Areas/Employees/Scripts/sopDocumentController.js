'use strict';
SOPDocumentController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "cboService", "$routeParams", "$location", "$http", "$filter", '$controller'];
function SOPDocumentController(commonMessage, $scope, $rootScope, baseService, cboService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = "SOP Document";
    $scope.Action = 'Save';
    $scope.ModuleList = [];
    $scope.index = -1;
    $scope.sopDocuments = [];
    $scope.path = 'Employees/sopdocument/';
    $scope.getListUrl = $scope.path + 'getsopdocumentlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    //$scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'Sequence', 'UserName');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.sopDocuments = result.Rows;
                $scope.Clear();
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    //for tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    
    $scope.sopDocuments = [];
    $scope.getData();
    $scope.searchByList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        }
    ];
    $scope.sopDocument = {
        Id: null
        //, CompanyGroupId: null
        , SOPDocumentCategoryId: null
        , SOPDocumentSubCategoryId: null
        , DataSourceCategory: null
        , DocumentFormate: null
        , FileName: null
        , FileId: null
        , Sequence: null
        , Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , Description: null
        , Remarks: null
        , Active: true
        , Printable: false
        , IsSystemGenerated: false
        , ModuleId: null

        , UserRefCode: null
        , Purpose: null
        , CriticalId: null
        , DocumentTypeId: null
        , PreparationFrequencyPerMonth: null
        , PreparationTimeInMinutes: null
        , ReviewRequired: false
    };
    $scope.sopDocumentNew = Object.assign({}, $scope.sopDocument);

   // $scope.documentCategoryList = [];

    $scope.CriticalLevelData = [];
    $http({
        method: 'GET',
        url: $scope.path + 'GetCriticalLevelData'
    }).then(function successCallback(response) {
        $scope.CriticalLevelData = response.data;
    });

    $scope.DocumentTypeData = [];
    $http({
        method: 'GET',
        url: $scope.path + 'GetDocumentTypeData'
    }).then(function successCallback(response) {
        $scope.DocumentTypeData = response.data;
    });

    $scope.documentCategoryList = [];
    $http({
        method: 'GET',
        url: 'Employees/sopdocumentcategory/getcbo'
    }).then(function successCallback(response) {
        $scope.documentCategoryList = response.data;
        });

    $http({
        method: 'GET',
        url: 'Employees/sopdocument/getmodulelist/',
    }).then(function successCallback(response) {
        $scope.ModuleList = response.data;
    });

    $scope.documentSubCategoryList = [];
    $http({
        method: 'GET',
        url: 'Employees/sopdocumentsubcategory/getcbo'
    }).then(function successCallback(response) {
        $scope.documentSubCategoryList = response.data;
    });

    $scope.DataSourceCategoryList = [];
    cboService.getEnumCbo('enum/getdatasourcecategoryenumcbo', function (result) {
        $scope.DataSourceCategoryList = result;
    });

    $scope.DocumentFormateList = [];
    cboService.getEnumCbo('enum/getdocumentformateenumcbo', function (result) {
        $scope.DocumentFormateList = result;
    });
    //for GetSequence of SOPDocument
    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.sopDocumentNew.Sequence = response.data;
            });
    }
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.filedata = {};
        $scope.index = index;
        $scope.sopDocument = $scope.sopDocuments[$scope.index];
        $scope.sopDocumentNew = Object.assign({}, $scope.sopDocument);
        var filename = document.getElementById("uploadFile").value = $scope.sopDocument.FileName;
        $scope.filedata.name = $scope.sopDocument.FileName;
        $scope.Action = "Update";
        //Gride View all Table Selected
        $scope.LoadAllSelectedProcess();
        $scope.LoadAllSelectedDepartment();
        $scope.LoadAllSelectedLocation();
        $scope.LoadAllSelectedDocumentPreparedBy();
        $scope.LoadAllSelectedDocumentSource();
        // $scope.SelectedDepartmentList();
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.setTab(1);
        if ($scope.sopDocumentNew.IsSystemGenerated)
            $scope.enable = false;
        else
            $scope.enable = true;
    };

    $scope.enable = true;
    $scope.Enabledisablemodule = function () {
        if ($scope.sopDocumentNew.IsSystemGenerated)
            $scope.enable = false;
        else
            $scope.enable = true;
    }




    //File Attachment-----Start
    //Attach and File
    $scope.filedata = null;
    $("#uploadBtn").change(function () {
        $scope.filedata = this.files[0];
    });
    //File Download
    $scope.FileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.SOPActivityDocument + '/' + data.FileId + extention;
    };

    document.getElementById("uploadBtn").onchange = function () {
        var filename = document.getElementById("uploadFile").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile").value = res;
    };
     //Detach file  button Method and id confirmDocumentDelete
    $scope.DocumentRemove = function () {
        $scope.message_confirmation = 'Are you sure to remove this file?';
        angular.element(document.querySelector('#confirmDocumentDelete')).modal('show');
    };
    $scope.removeDocument = function () {
        angular.element(document.querySelector('#confirmDocumentDelete')).modal('hide');
        document.getElementById('uploadBtn').value = '';
        $scope.filedata = '';
        $scope.sopDocument.FileName = "";
        $scope.filedata = {};
        document.getElementById('uploadFile').value = "";
        $scope.getData();
    };
    //SOPDocument Detach file method
    $scope.confirmCloseDocumentDelete = function () {
        angular.element(document.querySelector('#confirmDocumentDelete')).modal('hide');
    };
    // Clear Method for SOPDocument
    $scope.ClearDocument = function () {
        document.getElementById('uploadBtn').value = '';
        $scope.filedata = '';
        $scope.sopDocument.FileName = "";
        $scope.filedata = {};
        document.getElementById('uploadFile').value = "";
    };

    

    //File Attachment-----End

    // Save, Update Method for SOPDocument
    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.sopDocumentNewForm.$valid) {

                if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                    throw $scope.filedata.name + ' File size must be below 2 mb.';
                var fileName = null;
                if (!baseService.isUndefinedOrNull($scope.filedata))
                    fileName = $scope.filedata.name;
                $scope.sopDocumentNew.FileName = fileName;
                if (!baseService.isUndefinedOrNull($scope.sopDocumentNew.FileName)) {
                    if ($scope.sopDocumentNew.FileName.length > 50) {
                        throw "File Name must be less than 50 character."
                    }
                }
                var formData = new FormData();
                if (baseService.isUndefinedOrNull($scope.sopDocumentNew.FileName)) {
                    throw "Attachment is mandatory.";
                }
                angular.copy($scope.sopDocumentNew, $scope.sopDocument);
                if ($scope.Action == "Save" || $scope.Action == "Update") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        headers: { 'Content-Type': undefined },
                        transformRequest: function (data) {
                            formData.append("sopDocument", angular.toJson(data.sopDocument));
                            if (baseService.isUndefinedOrNull($scope.filedata) == false) {
                                formData.append('file', data.file);
                            }
                            return formData;
                        },
                        data: { 'sopDocument': $scope.sopDocument, 'file': $scope.filedata }
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.sopDocument = response.data.SOPDocument;
                            $scope.sopDocuments.push($scope.sopDocument);
                            $scope.sopDocuments = $filter('orderBy')($scope.sopDocuments, 'Sequence');

                            $scope.sopDocumentNew.Id = response.data.SOPDocument.Id;
                            //ClearFields(response.data.Sequence);
                            //$scope.ClearDocument();
                            $scope.getData();
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, "failure");
                    });
                    return true;
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    }
    //Delete Method for SOPDocument
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.sopDocumentNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.sopDocumentNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.sopDocuments.splice($scope.index, 1);
                    ClearFields(response.data.Sequence);
                    $scope.ClearDocument();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
        return true;
    }
    //Base Clear Method for SOPDocument
    $scope.Clear = function () {
        ClearFields(null);
      
        $scope.ClearDocument();
        return true;
    }
    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.sopDocument = {};
        $scope.sopDocumentNew = {};
        $scope.sopDocumentNew = { Sequence: seq, Active: true, Printable: false, ReviewRequired: false };

        $scope.SelectedProcessList = [];
        $scope.SelectedDepartmentList = [];
        $scope.SelectedLocationList = [];
        $scope.SelectedDocumentSourceList = [];
        $scope.SelectedDocumentPreparedByList = [];
        $scope.GetSequence();
        $scope.setTab();
    }


     //multiple process List handling

    $scope.closePopup = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
   
    //Get data from hkp.process
    $scope.ProcessList = [];
    $scope.showProcessPopUp = function () {
        angular.element(document.querySelector("#ProcessPopUp")).modal("show"); //Process popup id ProcessPopUp
        $scope.getProcessData();
    }
    $scope.getProcessData = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'LoadAllProcessForSelection?SOPDocumentId=' + $scope.sopDocumentNew.Id
        }).then(function successCallback(response) {
            $scope.ProcessList = response.data;
        });

    }
    //$scope.processrowcolor = function (args) {
    //    if (args.data.Active == false)
    //        args.row.css("background-color", "#ff0000");
    //}

    // Data Gride View PopUp from hkp.Process, LoadAllDepartmentForSelection table
    $scope.SelectedProcessList = [];
    $scope.LoadAllSelectedProcess = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'LoadAllSelectedProcess?SOPDocumentId=' + $scope.sopDocumentNew.Id
        }).then(function successCallback(response) {
            $scope.SelectedProcessList = response.data;
        });
    }
    //Save Function In Process,sopdocumentprocess Table
    $scope.SOPDocumentProcessId = '';
    $scope.SaveProcess = function () {

        var checkedData = [];
        for (var i = 0; i < $scope.ProcessList.length; i++) {
            if ($scope.ProcessList[i].isSelected == true)
                checkedData.push($scope.ProcessList[i]);
        }
        try {
            if (checkedData.length == 0) {
                throw 'Please select at least one process';
            }

            $http({
                method: 'POST',
                data: { SOPDocumentId: $scope.sopDocumentNew.Id, ProcessData: checkedData },
                url: $scope.path + 'SaveProcess'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.LoadAllSelectedProcess();
                }

            });
        }
        catch (e) {
            ShowResult(e, "failure");
        }


    }
    // delete gride view , SOPDocumentProcess
    $scope.DeleteProcess = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DeleteSelectedProcess?Id=' + $scope.SOPDocumentProcessId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.LoadAllSelectedProcess();
            }

        });
    }
    //Tab Gride View Delete method  ConfirmDeleteProcess from SOPDocuemntProcess
    $scope.ConfirmDeleteProcess = function (Id) {
        $scope.SOPDocumentProcessId = Id;
        angular.element(document.querySelector("#DeleteProcessPopUp")).modal("show"); //DeleteProcess from hkp.process and id DeleteProcessPopUp
    }

    //multiple Department List handling
    //Get data from org.department
    $scope.DepartmentList = [];
    $scope.showDepartmentPopUp = function () {
        angular.element(document.querySelector("#DepartmentPopUp")).modal("show");//Department popup id DepartmentPopUp
        $scope.getDepartmentData();
    }
    $scope.getDepartmentData = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'LoadAllDepartmentForSelection?SOPDocumentId=' + $scope.sopDocumentNew.Id
        }).then(function successCallback(response) {
            $scope.DepartmentList = response.data;
        });

    }

    //$scope.departmentrowcolor = function (args) {
    //    if (args.data.Active == false)
    //        args.row.css("background-color", "#ff0000");
    //}
    

    //Save Function In Department, SOPDocumentDepartment Table
    $scope.SOPDocumentDepartmentId = '';
    $scope.SaveDepartment = function () {



        var checkedData = [];
        for (var i = 0; i < $scope.DepartmentList.length; i++) {
            if ($scope.DepartmentList[i].isSelected == true)
                checkedData.push($scope.DepartmentList[i]);
        }
        try {
            if (checkedData.length == 0) {
                throw 'Please select at least one Department';
            }

            $http({
                method: 'POST',
                data: { SOPDocumentId: $scope.sopDocumentNew.Id, DepartmentData: checkedData },
                url: $scope.path + 'SaveDepartment'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.LoadAllSelectedDepartment();  //LoadAllSelectedProcess = LoadAllSelectedDepartment
                }

            });
        }
        catch (e) {
            ShowResult(e, "failure");
        }
    }

    // Loading for GriveView show SOPDocunmentDepartment
    $scope.SelectedDepartmentList = [];
    $scope.LoadAllSelectedDepartment = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'LoadAllSelectedDepartment?SOPDocumentId=' + $scope.sopDocumentNew.Id
        }).then(function successCallback(response) {
            $scope.SelectedDepartmentList = response.data;
        });
    }
    //Delete Function for SOPDocumentDepartment Table
    $scope.DeleteDepartment = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DeleteSelectedDepartment?Id=' + $scope.SOPDocumentDepartmentId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.LoadAllSelectedDepartment();
            }

        });
    }
    // gride view department 
    $scope.ConfirmDeleteDepartment = function (Id) {
        $scope.SOPDocumentDepartmentId = Id;
        angular.element(document.querySelector("#ConfirmDeleteDepartmentPopUp")).modal("show");
    }




    //multiple Location List handling
    //Get data location
    $scope.LocationList = [];
    $scope.showLocationPopUp = function () {
        angular.element(document.querySelector("#LocationPopUp")).modal("show");
        $scope.getLocationData();
    }
    $scope.getLocationData = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'LoadAllLocationForSelection?SOPDocumentId=' + $scope.sopDocumentNew.Id
        }).then(function successCallback(response) {
            $scope.LocationList = response.data;
        });

    }

    //$scope.locationrowcolor = function (args) {
    //    if (args.data.Active == false)
    //        args.row.css("background-color", "#ff0000");
    //}


    //Save Function In Location , SOPDocumentDepartment Table
    $scope.SOPDocumentLocationId = '';
    $scope.SaveLocation = function () {



        var checkedData = [];
        for (var i = 0; i < $scope.LocationList.length; i++) {
            if ($scope.LocationList[i].isSelected == true)
                checkedData.push($scope.LocationList[i]);
        }
        try {
            if (checkedData.length == 0) {
                throw 'Please select at least one Location';
            }

            $http({
                method: 'POST',
                data: { SOPDocumentId: $scope.sopDocumentNew.Id, LocationData: checkedData },
                url: $scope.path + 'SaveLocation'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.LoadAllSelectedLocation();  //LoadAllSelectedProcess = LoadAllSelectedLocation
                }

            });
        }
        catch (e) {
            ShowResult(e, "failure");
        }
    }

    // Loading for GriveView show location, SOPDocunmentLocation
    $scope.SelectedLocationList = [];
    $scope.LoadAllSelectedLocation = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'LoadAllSelectedLocation?SOPDocumentId=' + $scope.sopDocumentNew.Id
        }).then(function successCallback(response) {
            $scope.SelectedLocationList = response.data;
        });
    }
    //Delete Function for SOPDocumentLocation Table
    $scope.DeleteLocation = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DeleteSelectedLocation?Id=' + $scope.SOPDocumentLocationId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.LoadAllSelectedLocation();
            }

        });
    }

    $scope.ConfirmDeleteLocation = function (Id) {
        $scope.SOPDocumentLocationId = Id;
        angular.element(document.querySelector("#ConfirmDeleteLocationPopUp")).modal("show");
    }





    //Get data
    $scope.DocumentSourceList = [];
    $scope.showDocumentSourcePopUp = function () {
        angular.element(document.querySelector("#DocumentSourcePopUp")).modal("show");
        $scope.getDocumentSourceData();
    }
    $scope.getDocumentSourceData = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'LoadAllDocumentSourceForSelection?SOPDocumentId=' + $scope.sopDocumentNew.Id
        }).then(function successCallback(response) {
            $scope.DocumentSourceList = response.data;
        });

    }
    // Loading for GriveView show  DocumentSource, SOPDocumentSource table
    $scope.SelectedDocumentSourceList = [];
    $scope.LoadAllSelectedDocumentSource = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'LoadAllSelectedDocumentSource?SOPDocumentId=' + $scope.sopDocumentNew.Id
        }).then(function successCallback(response) {
            $scope.SelectedDocumentSourceList = response.data;
        });
    }
    //Save Function In sopDocumentDocumentSource Table
    $scope.SOPDocumentDocumentSourceId = '';
    $scope.SaveDocumentSource = function () {

        var checkedData = [];
        for (var i = 0; i < $scope.DocumentSourceList.length; i++) {
            if ($scope.DocumentSourceList[i].isSelected == true)
                checkedData.push($scope.DocumentSourceList[i]);
        }
        try {
            if (checkedData.length == 0) {
                throw 'Please select at least one DocumentSource';
            }

            $http({
                method: 'POST',
                data: { SOPDocumentId: $scope.sopDocumentNew.Id, DocumentSourceData: checkedData },
                url: $scope.path + 'SaveDocumentSource'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.LoadAllSelectedDocumentSource();
                }

            });
        }
        catch (e) {
            ShowResult(e, "failure");
        }


    }
    $scope.DeleteDocumentSource = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DeleteSelectedDocumentSource?Id=' + $scope.SOPDocumentDocumentSourceId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.LoadAllSelectedDocumentSource();
            }

        });
    }

    $scope.ConfirmDeleteDocumentSource = function (Id) {
        $scope.SOPDocumentDocumentSourceId = Id;
        angular.element(document.querySelector("#DeleteDocumentSourcePopUp")).modal("show");
    }



    //Get data
    $scope.EntryType = 'Employee';
    $scope.DocumentPreparedByList = [];
    $scope.showDocumentPreparedByPopUp = function () {
        angular.element(document.querySelector("#DocumentPreparedByPopUp")).modal("show");
        $scope.getDocumentPreparedByData();
    }
    $scope.getDocumentPreparedByData = function () {
        $scope.DocumentPreparedByList = [];
        var gridObj = $("#GridDocumentPreparedBy").data("ejGrid");
        if ($scope.EntryType == 'Employee')
            gridObj.showColumns("Employee Name");
        else
            gridObj.hideColumns("Employee Name");

        $http({
            method: 'POST',
            data: { EntryType: $scope.EntryType, SOPDocumentId: $scope.sopDocumentNew.Id },
            url: $scope.path + 'LoadAllDocumentPreparedByForSelection'
        }).then(function successCallback(response) {
            $scope.DocumentPreparedByList = response.data;
        });


    }

    //$scope.preparedByrowcolor = function (args) {
    //    if (args.data.EmployeeStatus != 'Active')
    //       args.row.css("background-color", "#ff0000");
    //}

    //$scope.preparedByrowcolor = function (e) {
    //    if (e.data.EmployeeStatus === 'Separated')
    //        e.row.css("background-color", "red");
    //};
    // Loading for GriveView show DocumentDocumentPreparedBy table

    

    $scope.SelectedDocumentPreparedByList = [];
    $scope.LoadAllSelectedDocumentPreparedBy = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'LoadAllSelectedDocumentPreparedBy?SOPDocumentId=' + $scope.sopDocumentNew.Id
        }).then(function successCallback(response) {
            $scope.SelectedDocumentPreparedByList = response.data;
        });
    }
    $scope.preparedByrowcolor = function (args) {
        if (args.data.EmployeeStatus != 'Active')
            args.row.css("background-color", "#ff0000");
    }
    //Save Function In  sopDocumentPreparedBy Table
    $scope.SOPDocumentDocumentPreparedById = '';
    $scope.SaveDocumentPreparedBy = function () {

        var checkedData = [];
        for (var i = 0; i < $scope.DocumentPreparedByList.length; i++) {
            if ($scope.DocumentPreparedByList[i].isSelected == true)
                checkedData.push($scope.DocumentPreparedByList[i]);
        }
        try {
            if (checkedData.length == 0) {
                throw 'Please select at least one Document Prepared By';
            }

            $http({
                method: 'POST',
                data: { SOPDocumentId: $scope.sopDocumentNew.Id, DocumentPreparedByData: checkedData, EntryType: $scope.EntryType },
                url: $scope.path + 'SaveDocumentPreparedBy'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.LoadAllSelectedDocumentPreparedBy();
                }

            });
        }
        catch (e) {
            ShowResult(e, "failure");
        }


    }
    $scope.DeleteDocumentPreparedBy = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DeleteSelectedDocumentPreparedBy?Id=' + $scope.SOPDocumentDocumentPreparedById
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.LoadAllSelectedDocumentPreparedBy();
            }

        });
    }

    $scope.ConfirmDeleteDocumentPreparedBy = function (Id) {
        $scope.SOPDocumentDocumentPreparedById = Id;
        angular.element(document.querySelector("#DeleteDocumentPreparedByPopUp")).modal("show");
    }

}

