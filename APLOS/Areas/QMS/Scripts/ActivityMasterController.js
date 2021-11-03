'use strict';
ActivityMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ActivityMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'QMS Activity Master';
    $scope.ModelList = [];
  
    $scope.SOPItemList = [];
    $scope.QMSActivityCategoryList = [];
    $scope.BusinessProcessTypeList = [];
    $scope.BusinessProcessList = [];
    $scope.QualityActivityCheckTypeList = [];

    

    $scope.path = 'QMS/ActivityMaster/';

    $scope.getListUrl = $scope.path + 'getlist';

    $scope.getSeqUrl = $scope.path + 'getautosequence';

    $scope.saveUrl = $scope.path + 'create';

    $scope.deleteUrl = $scope.path + 'delete/';
  
  

    baseService.init($scope.getListUrl);


    $scope.searchBy = "UserName"; $scope.search = "";
   

    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'QMSActivityCategoryId', name: "QMS Activity Category" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'CriticalityLevel', name: "Criticality Level" }, { value: 'BusinessProcessId', name: "Business Process" }];
 

    // #region ddl

    $http({
        method: 'GET',
        url: 'QMS/ActivityMaster/getcbosop/',
    }).then(function successCallback(response) {
        $scope.SOPItemList = response.data;
    });

    $http({
        method: 'GET',
        url: 'QMS/ActivityMaster/getcboqmsactivitycategorylist/',
    }).then(function successCallback(response) {
        $scope.QMSActivityCategoryList = response.data;
    });

    $http({
        method: 'GET',
        url: 'QMS/ActivityMaster/getcbobusinessprocesstypelist/',
    }).then(function successCallback(response) {
        $scope.BusinessProcessTypeList = response.data;
    });

    $http({
        method: 'GET',
        url: 'QMS/ActivityMaster/getcbobusinessprocesslist/',
    }).then(function successCallback(response) {
        $scope.BusinessProcessList = response.data;
    });

    $http({
        method: 'GET',
        url: 'QMS/ActivityMaster/getqualityactivitychecktypelist/',
    }).then(function successCallback(response) {
        $scope.QualityActivityCheckTypeList = response.data;
    });


    // #end region

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetSequence();
        });
    }
        $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        SOPId: null,
        QMSActivityCategoryId: null,
        CriticalityLevel: null,
        QualityActivityCheckTypeId: null,
        BusinessProcessId: null,
        BusinessProcessTypeId: null,
        AuditFrequency: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Remarks: null,
  
};
    $scope.QMSActivityMaster = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.QMSActivityMaster.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {

        $scope.QMSActivityMaster = Object.assign({}, args.data);
        $scope.Action = 'Update';
        $scope.LoadAllSelectedProcessTab();
        $scope.LoadAllSelectedDepartmentTab();
        $scope.LoadAllSelectedDocumentPreparedBy();
        $scope.LoadAllSelectedDocumentTab();
        $scope.LoadAllSelectedSubLocationTab();
        $scope.setTab(1);
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();            
        }
    };
    $scope.Action = 'Save';

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
        if ($scope.General.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.QMSActivityMaster },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.QMSActivityMaster = response.data.Data;
                    $scope.LoadAllSelectedProcessTab();
                    $scope.LoadAllSelectedDepartmentTab();
                    $scope.LoadAllSelectedDocumentPreparedBy();
                    $scope.LoadAllSelectedDocumentTab();
                    $scope.LoadAllSelectedSubLocationTab();
                    $scope.Action = 'Update';
                    $scope.Getgrid();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.QMSActivityMaster.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.QMSActivityMaster.Id,
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

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
       
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.QMSActivityMaster = Object.assign({}, $scope.ModelTemp);
        $scope.QMSActivityMaster.Sequence = seq;
        $scope.LoadAllSelectedProcessTab();
        $scope.LoadAllSelectedDepartmentTab();
        $scope.LoadAllSelectedDocumentPreparedBy();
        $scope.LoadAllSelectedDocumentTab();
        $scope.LoadAllSelectedSubLocationTab();
        $scope.setTab();
      
    }


  //  // Process Tab
    // **********************************************************

    // #region Process Tab

 
    $scope.ProcessList = [];
    $scope.showProcessTabPopUp = function () {
        angular.element(document.querySelector("#ProcessTabPopUp")).modal("show");
        $scope.getProcessTabData();
      
    }
    $scope.getProcessTabData = function () {
        $scope.ProcessList = [];

        $http({
            method: 'POST',
            data: { QMSActivityMasterId: $scope.QMSActivityMaster.Id },
            url: $scope.path + 'LoadAllProcessTabForSelection'
        }).then(function successCallback(response) {
            $scope.ProcessList = response.data;
        });
    }

    $scope.SelectedProcessTabList = [];
    $scope.LoadAllSelectedProcessTab = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'LoadAllSelectedProcessTab?QMSActivityMasterId=' + $scope.QMSActivityMaster.Id
        }).then(function successCallback(response) {
            $scope.SelectedProcessTabList = response.data;
        });
    }


    //Save Function In QmsProcess Table
    $scope.ProcessTabId = '';
    $scope.SaveProcessTab = function () {

        var checkedData = [];
        for (var i = 0; i < $scope.ProcessList.length; i++) {
            if ($scope.ProcessList[i].isSelected == true)
                checkedData.push($scope.ProcessList[i]);
        }
  

        try {
            if (checkedData.length == 0) {
                throw 'Please select at least one Process';
            }
    

            $http({
                method: 'POST',
                data: { QMSActivityMasterId: $scope.QMSActivityMaster.Id, ProcessTabData: checkedData },
                url: $scope.path + 'SaveProcessTab'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.LoadAllSelectedProcessTab();
                }

            });
        }
        catch (e) {
            ShowResult(e, "failure");
        }


    }
    $scope.DeleteProcess = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DeleteSelectedProcessTab?Id=' + $scope.ProcessTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.LoadAllSelectedProcessTab();
            }

        });
    }

    $scope.ConfirmDeleteProcessTab = function (Id) {
        $scope.ProcessTabId = Id;
        angular.element(document.querySelector("#DeleteProcessTabPopUp")).modal("show");
    }

    $scope.closeProcessTabPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    // # end region Process Tab

 //   ********************* ProcessTab End ***********************************


    //  ********************* Department Tab***********************************
    // #region Department Tab

   
    $scope.DepartmentList = [];
    $scope.showDepartmentTabPopUp = function () {
        angular.element(document.querySelector("#DepartmentTabPopUp")).modal("show");
        $scope.getDepartmentTabData();

    }
    $scope.getDepartmentTabData = function () {
        $scope.DepartmentList = [];

        $http({
            method: 'POST',
            data: { QMSActivityMasterId: $scope.QMSActivityMaster.Id },
            url: $scope.path + 'LoadAllDepartmentTabForSelection'
        }).then(function successCallback(response) {
            $scope.DepartmentList = response.data;
        });
    }

    $scope.SelectedDepartmentTabList = [];
    $scope.LoadAllSelectedDepartmentTab = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'LoadAllSelectedDepartmentTab?QMSActivityMasterId=' + $scope.QMSActivityMaster.Id
        }).then(function successCallback(response) {
            $scope.SelectedDepartmentTabList = response.data;
        });
    }


    //Save Function In QmsDepartment Table
    $scope.DepartmentTabId = '';
    $scope.SaveDepartmentTab = function () {

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
                data: { QMSActivityMasterId: $scope.QMSActivityMaster.Id, DepartmentTabData: checkedData },
                url: $scope.path + 'SaveDepartmentTab'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.LoadAllSelectedDepartmentTab();
                }

            });
        }
        catch (e) {
            ShowResult(e, "failure");
        }


    }
    $scope.DeleteDepartmentTab = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DeleteSelectedDepartmentTab?Id=' + $scope.DepartmentTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.LoadAllSelectedDepartmentTab();
            }

        });
    }

    $scope.ConfirmDeleteDepartmentTab = function (Id) {
        $scope.DepartmentTabId = Id;
        angular.element(document.querySelector("#confirmDelDepartmentPopUp")).modal("show");
    }

    $scope.closeDepartmentTabPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    // # end region Department Tab
  

    //***********Department Tab End**********************


    //  ********************* Sub Location Tab***********************************

    // #region Sub Location Tab


    $scope.SubLocationList = [];
    $scope.GetSubLocationDetails = function () {
        angular.element(document.querySelector("#SubLocationPopUp")).modal("show");
        $scope.getSubLocationTabData();

    }
    $scope.getSubLocationTabData = function () {
        $scope.SubLocationList = [];

        $http({
            method: 'POST',
            data: { QMSActivityMasterId: $scope.QMSActivityMaster.Id },
            url: $scope.path + 'LoadAllSubLocationTabForSelection'
        }).then(function successCallback(response) {
            $scope.SubLocationList = response.data;
        });
    }

    $scope.QmsSubLocationList = [];
    $scope.LoadAllSelectedSubLocationTab = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'LoadAllSelectedSubLocationTab?QMSActivityMasterId=' + $scope.QMSActivityMaster.Id
        }).then(function successCallback(response) {
            $scope.QmsSubLocationList = response.data;
        });
    }
    //Save Function In QmsSubLocation Table
    $scope.SubLocationTabId = '';
    $scope.SaveSubLocationTab = function () {

        var checkedData = [];
        for (var i = 0; i < $scope.SubLocationList.length; i++) {
            if ($scope.SubLocationList[i].isSelected == true)
                checkedData.push($scope.SubLocationList[i]);
        }
        try {
            if (checkedData.length == 0) {
                throw 'Please select at least one Sub Location';
            }

            $http({
                method: 'POST',
                data: { QMSActivityMasterId: $scope.QMSActivityMaster.Id, SubLocationTabData: checkedData },
                url: $scope.path + 'SaveSubLocationTab'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.LoadAllSelectedSubLocationTab();
                }

            });
        }
        catch (e) {
            ShowResult(e, "failure");
        }


    }
    $scope.DeleteSubLocation = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DeleteSubLocationTab?Id=' + $scope.SubLocationTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.LoadAllSelectedSubLocationTab();
            }

        });
    }

    $scope.ConfirmDeleteSubLocationTab = function (Id) {
        $scope.SubLocationTabId = Id;
        angular.element(document.querySelector("#removerSLPopUp")).modal("show");
    }

    $scope.closeSubLocationTabPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    // # end region Sub Location Tab

    //*********** Sub Location Tab End**********************


    //*********** Responsible Person Tab **********************
    // #region ResPerson

    $scope.EntryType = 'Employee';
    $scope.DocumentPreparedByList = [];
    $scope.showDocumentPreparedByPopUp = function () {
        angular.element(document.querySelector("#DocumentPreparedByPopUp")).modal("show");
       $scope.getDocumentPreparedByData();
      
    }
    $scope.getDocumentPreparedByData = function () {
        $scope.DocumentPreparedByList = [];
        var gridObj = $("#GridPreparedBy").data("ejGrid");
        if ($scope.EntryType == 'Employee')
            gridObj.showColumns("Employee Name");
        else
            gridObj.hideColumns("Employee Name");

        $http({
            method: 'POST',
            data: { EntryType: $scope.EntryType, QMSActivityMasterId: $scope.QMSActivityMaster.Id },
            url: $scope.path + 'LoadAllDocumentPreparedByForSelection'
        }).then(function successCallback(response) {
            $scope.DocumentPreparedByList = response.data;
        });
    }
    $scope.preparedByrowcolor = function (args) {
        if (args.data.EmployeeStatus != 'Active')
            args.row.css("background-color", "#ff0000");
    }

    $scope.SelectedDocumentPreparedByList = [];
    $scope.LoadAllSelectedDocumentPreparedBy = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'LoadAllSelectedDocumentPreparedBy?QMSActivityMasterId=' + $scope.QMSActivityMaster.Id
        }).then(function successCallback(response) {
            $scope.SelectedDocumentPreparedByList = response.data;
        });
    }
    //Save Function In QmsResponsible Person Table
    $scope.SOPDocumentDocumentPreparedById = '';
    $scope.SaveResponsiblePerson = function () {

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
                data: { QMSActivityMasterId: $scope.QMSActivityMaster.Id, DocumentPreparedByData: checkedData, EntryType: $scope.EntryType },
                url: $scope.path + 'SaveResponsiblePerson'
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

    $scope.closeResponsiblePersonPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    // # end region ResPerson

    ///////*********************Tabs*******************************
    // #region Tab
    //  $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #endregion

    //***** Document Tab**********

    // #region Document Tab


    $scope.SubDocList = [];
    $scope.GetDocumentDetails = function () {
        angular.element(document.querySelector("#SubDocumentPopUp")).modal("show");
        $scope.getDocumentTabData();

    }
    $scope.getDocumentTabData = function () {
        $scope.SubDocList = [];

        $http({
            method: 'POST',
            data: { QMSActivityMasterId: $scope.QMSActivityMaster.Id },
            url: $scope.path + 'LoadAllDocumentTabForSelection'
        }).then(function successCallback(response) {
            $scope.SubDocList = response.data;
        });
    }

    $scope.QmsDocumentList = [];
    $scope.LoadAllSelectedDocumentTab = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'LoadAllSelectedDocumentTab?QMSActivityMasterId=' + $scope.QMSActivityMaster.Id
        }).then(function successCallback(response) {
            $scope.QmsDocumentList = response.data;
        });
    }
    //Save Function In QmsDocument Table
    $scope.DocumenntTabId = '';
    $scope.SaveDocumentTab = function () {
        
            var checkedData = [];
            for (var i = 0; i < $scope.SubDocList.length; i++) {
                if ($scope.SubDocList[i].isSelected == true)
                    checkedData.push($scope.SubDocList[i]);
            }
        try {
            if (checkedData.length == 0) {
                throw 'Please select at least one Document';
            }


                    $http({
                        method: 'POST',
                        data: { QMSActivityMasterId: $scope.QMSActivityMaster.Id, DocumentTabData: checkedData },
                        url: $scope.path + 'SaveDocumentTab'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.LoadAllSelectedDocumentTab();
                        }


                    });
           
        }
             
               
        catch (e) {
            ShowResult(e, "failure");
        }


    }
    $scope.DeleteDocument = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DeleteDocumentTab?Id=' + $scope.DocumenntTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.LoadAllSelectedDocumentTab();
            }

        });
    }

    $scope.ConfirmDeleteDocumentTab = function (Id) {
        $scope.DocumenntTabId = Id;
        angular.element(document.querySelector("#removerPopUp")).modal("show");
    }

    $scope.closeDocumentTabPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }


    // # end region Document Tab

   
}