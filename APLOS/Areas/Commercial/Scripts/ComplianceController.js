'use strict';
ComplianceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ComplianceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Compliance';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Commercial/Compliance/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.Action = 'Save';
    $scope.valueData = '';
    $scope.message = null;
    $scope.imageSrc = null;
    $scope.filedata = null;
    $scope.fileName = '';
    $scope.fileSize = '';


    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tab2 = 1;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };

    $scope.searchBy = "Code"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'Remarks', name: "Remarks" }];

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

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };

    $scope.ModelTemp = {
        Id: null,
        ComplianceGroup: null,
        Code: null,
        Category: null,
        SubCategory: null,
        ItemName: null,
        CriticalityLevel: null,
        ComplianceValue: null,
        Remarks: null,
        LocationReference: null,
        ScanApplicable: null,
        CodeApplicable: null,
        IsDocumentVerification: null,
        Image: null,
        imageSrc: null,
        fileAttachment: null,
        ExpiryDate: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.ModelNew.ComplianceValue = $scope.ModelNew.ComplianceValue.toString();
        $scope.GetRPList();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.setFile = function (element) {
        var file = element.files[0];
        if (file) {
            $scope.$apply(function () {
                $scope.filedata = file;
                $scope.fileName = file.name;
                $scope.fileSize = formatFileSize(file.size);
            });
        } else {
            $scope.$apply(function () {
                $scope.filedata = null;
                $scope.fileName = '';
                $scope.fileSize = '';
            });
        }
    };
    // Download file function
    $scope.downloadFile = function (id) {
        if (!id) {
            alert('No record selected');
            return;
        }

        // Direct download approach
        var url = '/Commercial/Compliance/DownloadFile?id=' + encodeURIComponent(id);
        window.open(url, '_blank');
    };


    // Helper function to format file size
    function formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        var k = 1024;
        var sizes = ['Bytes', 'KB', 'MB', 'GB'];
        var i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }

    // MAIN SAVE FUNCTION - UPDATED
    $scope.Save = function () {
        console.log('=== SAVE FUNCTION STARTED ===');

        $scope.$broadcast('show-errors-check-validity');

        if ($scope.ModelNewForm.$valid) {
            console.log('Form is valid');
            console.log('Model data:', $scope.ModelNew);

            // Validate file if exists
            var fileInput = document.getElementById('fileAttachment');
            var fileData = null;

            if (fileInput && fileInput.files.length > 0) {
                fileData = fileInput.files[0];
                console.log('File selected:', fileData.name, 'Size:', fileData.size);

                if (fileData.size > 2097152) {
                    ShowResult(fileData.name + ' - File size must be below 2 MB', 'failure');
                    return;
                }
            } else {
                console.log('No file selected');
            }

            // Create FormData
            var formData = new FormData();

            // Add file if exists
            if (fileData) {
                formData.append('fileAttachment', fileData);
                console.log('File added to FormData');
            }

            // Add form data - ensure ModelNew has Id field
            if (!$scope.ModelNew.Id) {
                $scope.ModelNew.Id = "0"; // Set default ID for new records
            }

            console.log('Sending data:', $scope.ModelNew);
            formData.append('data', JSON.stringify($scope.ModelNew));

            // Show loading
            ShowResult('Saving... Please wait', 'info');

            // Send request
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: formData,
                headers: {
                    'Content-Type': undefined
                },
                transformRequest: angular.identity
            }).then(function successCallback(response) {
                console.log('Server response:', response.data);

                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                } else {
                    ShowResult(response.data.Message, 'success');
                    // Clear file input
                    if (fileInput) {
                        fileInput.value = '';
                    }
                    ClearFields();
                    $scope.getData();
                }
            }, function errorCallback(response) {
                console.log('Error response:', response);
                ShowResult(response.data ? response.data.Message : 'Error occurred while saving', 'failure');
            });
        } else {
            console.log('Form is invalid');
            ShowResult('Please fill all required fields correctly', 'failure');
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
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.RPDataList = [];

        // Clear file attachment
        clearFileAttachment();
    }

    // Function to clear file attachment
    function clearFileAttachment() {
        // Get the file input element
        var fileInput = document.getElementById('fileAttachment');

        if (fileInput) {
            // Reset the file input
            fileInput.value = '';

            // Clear any AngularJS file model if exists
            if ($scope.filedata) {
                $scope.filedata = null;
            }
            if ($scope.fileName) {
                $scope.fileName = '';
            }

            // Hide file name display if exists
            var fileNameDisplay = document.getElementById('fileNameDisplay');
            var selectedFileNameDiv = document.getElementById('selectedFileName');

            if (fileNameDisplay) {
                fileNameDisplay.textContent = '';
            }
            if (selectedFileNameDiv) {
                selectedFileNameDiv.style.display = 'none';
            }

            // Clear from ModelNew if exists
            if ($scope.ModelNew) {
                $scope.ModelNew.FileName = '';
                $scope.ModelNew.HasFile = false;
                $scope.ModelNew.FilePath = '';
                $scope.ModelNew.DeleteExistingFile = false;
            }

            console.log('File attachment cleared');
        } else {
            console.log('File input element not found');
        }
    }

    $scope.CloseResponsiblePerson = function () {
        angular.element(document.querySelector('#ResponsiblePersonPopUp')).modal('hide');

    }

    $scope.popUpDataList = [];
    $scope.name = null;
    $scope.popUp = function (name) {
        try {
            $scope.name = name;
            if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
                $scope.popUpDataList = [];
                $http({
                    method: 'GET',
                    url: 'employees/authorizationconfig/getallemployeedata'

                }).then(function successCallback(response) {
                    $scope.popUpDataList = response.data;
                });
                angular.element(document.querySelector('#popUp')).modal('show');
            }
            else {
                throw "Select Master data first.";
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.refreshTemplate = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAll });
    };

    function CheckBoxSelectAll(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridPopUp").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.popUpDataList.length; i++) {
                $scope.popUpDataList[i].Flag = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridPopUp").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    };

    $scope.RPDataList = [];
    $scope.GetRPList = function () {
        $http({
            method: 'GET',
            url: "Commercial/Compliance/GetRP?masterId=" + $scope.ModelNew.Id,
        }).then(function successCallback(response) {
            $scope.RPDataList = response.data;
            $scope.GetADList();
        });
    }

    $scope.ADDataList = [];
    $scope.GetADList = function () {
        $http({
            method: 'GET',
            url: "Commercial/Compliance/GetAuditorData?masterId=" + $scope.ModelNew.Id,
        }).then(function successCallback(response) {
            $scope.ADDataList = response.data;
        });
    }

    $scope.SaveRP = function () {
        if ($scope.name == "RP") {
            for (var i = 0; i < $scope.popUpDataList.length; i++) {
                if ($scope.popUpDataList[i].Flag == true) {
                    if (checkExists($scope.RPDataList, $scope.popUpDataList[i].SystemId) === false) {
                        var ob = {};
                        ob.Id = Math.floor(Math.random() * 9) - 10;
                        ob.ComplianceMasterId = $scope.ModelNew.Id;
                        ob.EmpSystemID = $scope.popUpDataList[i].SystemId;
                        ob.EmployeeCode = $scope.popUpDataList[i].EmployeeCode;
                        ob.EmployeeName = $scope.popUpDataList[i].EmployeeName;
                        ob.Plant = $scope.popUpDataList[i].Plant;
                        ob.LegalDesignation = $scope.popUpDataList[i].LegalDesignation;
                        ob.Department = $scope.popUpDataList[i].Department;
                        ob.Section = $scope.popUpDataList[i].Section;
                        ob.SubSection = $scope.popUpDataList[i].SubSection;
                        ob.Line = $scope.popUpDataList[i].Line;
                        ob.SourceType = "ResponsiblePerson";
                        $scope.RPDataList.push(ob);
                    }
                }
            }
            if ($scope.RPDataList.length > 0) {
                $http({
                    method: 'POST',
                    url: 'Commercial/Compliance/CreateRP',
                    data: { 'RPDataList': $scope.RPDataList, 'masterId': $scope.ModelNew.Id },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetRPList();
                        $scope.closePopUp();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        }
        else {
            for (var i = 0; i < $scope.popUpDataList.length; i++) {
                if ($scope.popUpDataList[i].Flag == true) {
                    if (checkExistsAD($scope.ADDataList, $scope.popUpDataList[i].SystemId) === false) {
                        var ob = {};
                        ob.Id = Math.floor(Math.random() * 9) - 10;
                        ob.ComplianceMasterId = $scope.ModelNew.Id;
                        ob.EmpSystemID = $scope.popUpDataList[i].SystemId;
                        ob.EmployeeCode = $scope.popUpDataList[i].EmployeeCode;
                        ob.EmployeeName = $scope.popUpDataList[i].EmployeeName;
                        ob.Plant = $scope.popUpDataList[i].Plant;
                        ob.LegalDesignation = $scope.popUpDataList[i].LegalDesignation;
                        ob.Department = $scope.popUpDataList[i].Department;
                        ob.Section = $scope.popUpDataList[i].Section;
                        ob.SubSection = $scope.popUpDataList[i].SubSection;
                        ob.Line = $scope.popUpDataList[i].Line;
                        ob.SourceType = "Auditor";
                        $scope.ADDataList.push(ob);
                    }
                }
            }
            if ($scope.ADDataList.length > 0) {
                $http({
                    method: 'POST',
                    url: 'Commercial/Compliance/CreateAD',
                    data: { 'RPDataList': $scope.ADDataList, 'masterId': $scope.ModelNew.Id },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetADList();
                        $scope.closePopUp();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        }

    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmpSystemID === id) {
                return true;
            }
        }
        return false;
    }

    function checkExistsAD(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmpSystemID === id) {
                return true;
            }
        }
        return false;
    }

   

    $scope.message_detailconfirmation = null;
    $scope.removeRP = function (obj) {
        $scope.bomDetailNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.bomDetailNew.Id))
            $scope.message_detailconfirmation = 'Are you sure want to delete permanently [ ' + $scope.bomDetailNew.EmployeeCode + ' ]';
        angular.element(document.querySelector('#confirmBoMDetailPopUp')).modal('show');
    }

    $scope.removeAD = function (obj) {
        $scope.bomDetailNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.bomDetailNew.Id))
            $scope.message_detailconfirmation = 'Are you sure want to delete permanently [ ' + $scope.bomDetailNew.EmployeeCode + ' ]';
        angular.element(document.querySelector('#confirmBoMDetailPopUp')).modal('show');
    }

    $scope.DeleteRP = function () {
        $http({
            method: 'POST',
            url: 'Commercial/Compliance/DeleteRP?id=' + $scope.bomDetailNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetRPList();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    $scope.CheckPointsList = [];
    $scope.showCheckPointsPopUp = function () {
        $scope.GetCheckPointsList();
        angular.element(document.querySelector('#CheckPointsPopUp')).modal('show');

    }

    $scope.CloseCheckPointsPopUp = function () {
        angular.element(document.querySelector('#CheckPointsPopUp')).modal('hide');

    }

    $scope.GetCheckPointsList = function () {
        $scope.CheckPointsList = [];
        $http.get('Commercial/Compliance/GetComplianceCheckPointsData?masterId=' + $scope.ModelNew.Id)
            .then(function (response) {
                $scope.CheckPointsList = response.data;
            });
    }

    $scope.CheckPointsModel = { Id: null, ComplianceMasterId: null, CheckPointName: null }

    $scope.SaveCheckPoints = function (model) {
        try {
            model.data.ComplianceMasterId = $scope.ModelNew.Id;
            $http({
                method: 'POST',
                data: { data: model.data },
                url: 'Commercial/Compliance/CreateCheckPoint'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.CheckPointsModel = { Id: null, ComplianceMasterId: null, CheckPointName: null }
                    $scope.GetCheckPointsList();
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.DeleteCheckPoints = function (model) {
        try {
            $http({
                method: 'POST',
                data: { id: model.data.Id },
                url: 'Commercial/Compliance/DeleteCheckPoint'
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetCheckPointsList();
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    //#region Group
    $scope.etype = "";
    $scope.GroupModelList = [];
    $scope.GroupsearchBy = "UserName"; $scope.search = "";
    $scope.GroupsearchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getGroupData = function (etype) {
        $scope.etype = etype;
        $http({
            method: 'POST',
            url: $scope.path + "GetDataList",
            data: { column: $scope.GroupsearchBy, value: $scope.searchGroup, 'entryType': $scope.etype },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.GroupModelList = response.data;
            ClearGroupFields(response.data.Sequence);
            $scope.GetGroupSequence();
        });
    }

    $scope.getGrpData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetDataList",
            data: { column: $scope.GroupsearchBy, value: $scope.searchGroup, 'entryType': $scope.etype },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.GroupModelList = response.data;
            ClearGroupFields(response.data.Sequence);
            $scope.GetGroupSequence();
        });
    }

    $scope.GroupModelTemp = {
        Id: null,
        EntryType: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.GroupModelNew = Object.assign({}, $scope.GroupModelTemp);

    $scope.GetGroupSequence = function () {
        $http.get('Commercial/Compliance/GetCategoryTypeAutoSequence?entryType=' + $scope.etype)
            .then(function (response) {
                $scope.GroupModelNew.Sequence = response.data;
            });
    };

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


    $scope.GetGroup = function (args) {
        $scope.GroupModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveGroup = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.GroupModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: 'Commercial/Compliance/CreateData',
                data: { 'data': $scope.GroupModelNew, 'entryType': $scope.etype },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearGroupFields(response.data.Sequence);
                    $scope.getGroupData($scope.etype);

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.message_confirmation = null;
    $scope.removeGroup = function () {
        if (!baseService.isUndefinedOrNull($scope.GroupModelNew.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently [ ' + $scope.GroupModelNew.UserName + ' ]';
        angular.element(document.querySelector('#confirmDataPopUp')).modal('show');
    }

    $scope.deleteGUrl = "Commercial/Compliance/DeleteData";

    $scope.DeleteGroup = function () {
        if (!baseService.isUndefinedOrNull($scope.GroupModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteGUrl,
                data: { 'id': $scope.GroupModelNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearGroup();
                    $scope.getGrpData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.ClearGroup = function () {
        ClearGroupFields($scope.GetGroupSequence($scope.etype));
        return true;
    };

    function ClearGroupFields(seq) {
        $scope.Action = 'Save';
        $scope.GroupModelNew = Object.assign({}, $scope.GroupModelTemp);
        $scope.GroupModelNew.Sequence = seq;
    }

    //#endregion

    //#region Category

    $scope.CategoryModelList = [];
    $scope.CategorysearchBy = "UserName"; $scope.search = "";
    $scope.CategorysearchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getCategoryData = function (etype) {
        $scope.etype = etype;
        $http({
            method: 'POST',
            url: $scope.path + "GetDataList",
            data: { column: $scope.CategorysearchBy, value: $scope.searchCategory, 'entryType': $scope.etype },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.CategoryModelList = response.data;
            ClearCategoryFields(response.data.Sequence);
            $scope.GetCategorySequence();
        });
    }


    $scope.getCatData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetDataList",
            data: { column: $scope.CategorysearchBy, value: $scope.searchCategory, 'entryType': $scope.etype },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.CategoryModelList = response.data;
            ClearCategoryFields(response.data.Sequence);
            $scope.GetCategorySequence();
        });
    }

    $scope.CategoryModelTemp = {
        Id: null,
        EntryType: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.CategoryModelNew = Object.assign({}, $scope.CategoryModelTemp);

    $scope.GetCategorySequence = function () {
        $http.get('Commercial/Compliance/GetCategoryTypeAutoSequence?entryType=' + $scope.etype)
            .then(function (response) {
                $scope.CategoryModelNew.Sequence = response.data;
            });
    };

    $scope.GetCategory = function (args) {

        $scope.CategoryModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveCategory = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.CategoryModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: 'Commercial/Compliance/CreateData',
                data: { 'data': $scope.CategoryModelNew, 'entryType': $scope.etype },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearCategoryFields(response.data.Sequence);
                    $scope.getCategoryData($scope.etype);

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };


    $scope.removeCategory = function () {
        if (!baseService.isUndefinedOrNull($scope.CategoryModelNew.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently [ ' + $scope.CategoryModelNew.UserName + ' ]';
        angular.element(document.querySelector('#confirmCatDataPopUp')).modal('show');
    }
    $scope.DeleteCategory = function () {
        if (!baseService.isUndefinedOrNull($scope.CategoryModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteGUrl,
                data: { 'id': $scope.CategoryModelNew.Id},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearCategory();
                    $scope.getCatData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.ClearCategory = function () {
        ClearFields($scope.GetCategorySequence($scope.etype));
        return true;
    };

    function ClearCategoryFields(seq) {
        $scope.Action = 'Save';
        $scope.CategoryModelNew = Object.assign({}, $scope.CategoryModelTemp);
        $scope.CategoryModelNew.Sequence = seq;
    }

    //#endregion

    //#region SubCategory

    $scope.SubCategoryModelList = [];
    $scope.SubCategorysearchBy = "UserName"; $scope.search = "";
    $scope.SubCategorysearchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getSubCategoryData = function (etype) {
        $scope.etype = etype;
        $http({
            method: 'POST',
            url: $scope.path + "GetDataList",
            data: { column: $scope.SubCategorysearchBy, value: $scope.searchSubCategory, 'entryType': $scope.etype },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.SubCategoryModelList = response.data;
            ClearSubCategoryFields(response.data.Sequence);
            $scope.GetSubCategorySequence();
        });
    }

    $scope.getSubCatData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetDataList",
            data: { column: $scope.SubCategorysearchBy, value: $scope.searchSubCategory, 'entryType': $scope.etype },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.SubCategoryModelList = response.data;
            ClearSubCategoryFields(response.data.Sequence);
            $scope.GetSubCategorySequence();
        });
    }

    $scope.SubCategoryModelTemp = {
        Id: null,
        EntryType: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.SubCategoryModelNew = Object.assign({}, $scope.SubCategoryModelTemp);

    $scope.GetSubCategorySequence = function () {
        $http.get('Commercial/Compliance/GetCategoryTypeAutoSequence?entryType=' + $scope.etype)
            .then(function (response) {
                $scope.SubCategoryModelNew.Sequence = response.data;
            });
    };

    $scope.GetSubCategory = function (args) {

        $scope.SubCategoryModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveSubCategory = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.SubCategoryModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: 'Commercial/Compliance/CreateData',
                data: { 'data': $scope.SubCategoryModelNew, 'entryType': $scope.etype },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearSubCategoryFields(response.data.Sequence);
                    $scope.getSubCategoryData($scope.etype);

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.removeSubCategory = function () {
        if (!baseService.isUndefinedOrNull($scope.SubCategoryModelNew.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently [ ' + $scope.SubCategoryModelNew.UserName + ' ]';
        angular.element(document.querySelector('#confirmSCDataPopUp')).modal('show');
    }
    $scope.DeleteSC = function () {
        if (!baseService.isUndefinedOrNull($scope.SubCategoryModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteGUrl,
                data: { 'id': $scope.SubCategoryModelNew.Id},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearSubCategory();
                    $scope.getSubCatData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.ClearSubCategory = function () {
        ClearFields($scope.GetSubCategorySequence());
        return true;
    };

    function ClearSubCategoryFields(seq) {
        $scope.Action = 'Save';
        $scope.SubCategoryModelNew = Object.assign({}, $scope.SubCategoryModelTemp);
        $scope.SubCategoryModelNew.Sequence = seq;
    }

    

    //#endregion

}