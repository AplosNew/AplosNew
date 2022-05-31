'use strict';
ResidenceStatusLocationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ResidenceStatusLocationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Residence Status Loacation';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/ResidenceStatusLocation/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    // Tab Change
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

    // All List Variables are here for dropdown
    $scope.PlantList = [];
    $scope.LocationList = [];
    $scope.ResidenceGroupIdList = [];
    $scope.ResidenceCategoryList = [];
    $scope.ResidenceSubCategoryList = [];
    $scope.BlockList = [];

    $scope.getPlant = function () {
        $http({
            method: 'POST',
            data: {            
                    'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
                },
            url: $scope.path + 'getPlant',
        }).then(function success(response) {
            $scope.PlantList = response.data;
        });
    }
   // $scope.getPlant();

    $scope.getLocation = function () {
        $http({
            method: 'POST',
            data: {
                'PlantId': $scope.selectedData.PlantId,
                'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
            },
            url: $scope.path + 'getLocation',
        }).then(function successCallback(response) {
            $scope.LocationList = [];
            $scope.LocationList = response.data;
        });
    }
    //$scope.getLocation();

    $scope.ResidenceGId = null;
    $scope.getResidenceGroup = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getResidenceGroup',
        }).then(function success(response) {
            $scope.ResidenceGroupIdList = response.data;
            
        });
    }
    $scope.getResidenceGroup();

    $scope.EmpServiceTypeList = [];
    $scope.getServiceType = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getServiceType',
            data: {
                'PlantId': $scope.selectedData.PlantId,
                'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
            },
        }).then(function success(response) {
            $scope.EmpServiceTypeList = response.data;
        });
    }
    //$scope.getResidenceCategory();

    $scope.getResidenceSubCategory = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getResidenceSubCategory',
            data: {
                'PlantId': $scope.selectedData.PlantId,
                'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
            },
        }).then(function success(response) {
            $scope.ResidenceSubCategoryList = response.data;
        });
    }
   // $scope.getResidenceSubCategory();

    $scope.getBlock = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getBlock',
            data: {
                'PlantId': $scope.selectedData.PlantId,
                'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
            },
        }).then(function success(response) {
            $scope.BlockList = response.data;
        });
    }
   // $scope.getBlock();

    $scope.RoomList = [];

    $scope.getRoom = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getRoom',
            data: {
                'PlantId': $scope.selectedData.PlantId,
                'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
            },
        }).then(function success(response) {
            $scope.RoomList = response.data;
        });
    }
    //$scope.getRoom();

    $scope.EmployeeTypeIdList = [];
    $scope.getEmployeeType = function () {
        $http({
            method: 'POST',
            data: {
                'PlantId': $scope.selectedData.PlantId,
                'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
            },
            url: $scope.path + 'getEmployeeType',
        }).then(function success(response) {
            $scope.EmployeeTypeIdList = response.data;
        });
    }
   // $scope.getEmployeeType();

    $scope.ResidenceNumberList = [];
    $scope.getResidenceNumber = function () {
        $http({
            method: 'POST',
            data: {
                'PlantId': $scope.selectedData.PlantId,
                'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
            },
            url: $scope.path + 'getResidenceNumber',
        }).then(function success(response) {
            $scope.ResidenceNumberList = response.data;
        });
    }
   // $scope.getResidenceNumber();

    $scope.FloreList = [];
    $scope.getFloor = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getFloor',
            data: {
                'PlantId': $scope.selectedData.PlantId,
                'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
            },
        }).then(function success(response) {
            $scope.FloreList = response.data;
        });
    }
    // $scope.getFloor();
    $scope.ResidentTypeList = [];
    $scope.getResidentType = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getResidentType',
            data: {
                'PlantId': $scope.selectedData.PlantId,
                'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
            },
        }).then(function success(response) {
            $scope.ResidentTypeList = response.data;
        });
    }

    $scope.AssetNameList = [];
    $scope.getAssetName = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getAssetName',
            data: {
                'PlantId': $scope.selectedData.PlantId,
                'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
            },
        }).then(function success(response) {
            $scope.AssetNameList = response.data;

            $scope.getVacancy();
        });
    }

    $scope.getVacancy = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getVacancy',
            data: {
                'PlantId': $scope.selectedData.PlantId,
                'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
            },
            dataType:'JSON',
        }).then(function success(response) {
            $scope.VacancyList = response.data;
        })
    }

    $scope.selectedData = {
        Id: null,      
        PlantId: null,
        ResidedenceGroupId: null,
        EmployeeCategoryId:null,
        Location: null,
        AssetName:null,
        ResidenceSubCategory: null,
        ResidenceCategory: null,
        Rooms: null,
        Block: null,
        ResidenceType: null,
        Floor: null,
        ResidenceType: null,
        ResidenceNumber: null,
        VacancyStatus: null,
        Vacancy:null,
        isActive: 0,
        afterDate: null,
        toDate:null,
    };

    $scope.view = function () {
       /* var ColumnList = [
            { field: 'isActive', width: 150, headerText: "IsActive", type: "boolean" },
            { field: 'Location', width: 150, headerText: "Location", type: "string" },
            { field: 'EmployeeCategoryId', width: 150, headerText: "Employee Category Id", type: "string" },
            { field: 'ResidenceSubCategory', width: 150, headerText: "Residence Sub Category", type: "string" },
            { field: 'ResidentType', width: 150, headerText: "Resident Type", type: "string" },
            { field: 'Block', width: 150, headerText: "Block", type: "string" },
            { field: 'Floor', width: 150, headerText: "Floor", type: "string" },
            { field: 'ResidenceNumber', width: 150, headerText: "ResidenceNumber", type: "string" },
            { field: 'Rooms', width: 150, headerText: "Rooms", type: "string" },
            { field: 'VacancyStatus', width: 150, headerText: "Vacancy Status", type: "string"},
            { field: 'Occupie', width: 150, headerText: "Occupie", type: "string"},
            { field: 'Available', width: 150, headerText: "Available", type: "string"},
            { field: 'Allocation', width: 150, headerText: "Allocation", type: "string"},
            { field: 'AssetName', width: 150, headerText: "AssetName", type: "string"},
            { field: 'Remarks', width: 150, headerText: "Remarks", type: "string"},
            { field: 'AddedBy', width: 150, headerText: "AddedBy", type: "string"},
            { field: 'AddedDate', width: 150, headerText: "AddedDate", type: "string" },
            
        ];*/
        $http({
            method: "GET",
            //url: $scope.path + 'view?PlantId=' + $scope.selectedData.PlantId + '&ResidenceGroupId=' + $scope.selectedData.ResidenceGroupId + '&EmployeeCategoryId=' + $scope.selectedData.EmployeeCategoryId,
            url: $scope.path + 'view?PlantId=' + $scope.selectedData.PlantId + '&ResidenceGroupId=' + $scope.selectedData.ResidenceGroupId ,
            dataType: 'JSON'
        }).then(function successCallback(response) {           
            $scope.ModelList = response.data;

            // Display Vacancy status
            //for (var i = 0; i < $scope.ModelList.length; i++) {
            //    $scope.ModelList[i].VacancyStatus = $scope.selectedData.VacancyStatus;

            //}
            $scope.getEmployee();
             
      
        })
    }

    $scope.popupEmployeeList = [];
    $scope.PopupEmployeeView = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'PopupEmployeeView',
            data: {
                'EmployeeCategorySystemID': $scope.selectedData.EmployeeCategoryId,
                'fromDate': $scope.selectedData.fromDate,
                'toDate': $scope.selectedData.toDate,
            }

        }).then(function successCallback(response) {
            $scope.popupEmployeeList = response.data;
            document.getElementById("EmpGrid").style.display = "block";
        })
    }

    $scope.selResidenceMasterId = null;
    $scope.selResidenceMaster = function (e) {
        $scope.selResidenceMasterId = e.data.Id;
        $scope.openChildGrid();
        $scope.getResidenceStatusLocation();
    }

    $scope.openChildGrid = function () {
        angular.element(document.querySelector('#EmpPop')).modal('show');
    }
    $scope.closeChildGrid = function () {
        angular.element(document.querySelector('#EmpPop')).modal('hide');
    }



    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.PlantList = [];
        $scope.LocationList = [];
        $scope.ResidenceGroupIdList = [];
        $scope.ResidenceCategoryList = [];
        $scope.ResidenceSubCategoryList = [];
        $scope.BlockList = [];
        $scope.AssetNameList = [];
        $scope.ResidentTypeList = [];
        $scope.FloreList = [];
        $scope.ResidenceNumberList = [];
        $scope.EmployeeTypeIdList = [];
        $scope.RoomList = [];
        $scope.selectedData = {
            Id: null,
            PlantId: null,
            ResidedenceGroupId: null,
            EmployeeCategoryId: null,
            Location: null,
            AssetName: null,
            ResidenceSubCategory: null,
            ResidenceCategory: null,
            Rooms: null,
            Block: null,
            ResidenceType: null,
            Floor: null,
            ResidenceType: null,
            ResidenceNumber: null,
            VacancyStatus: null,
            isActive: 0,
        };
       
    }

    $scope.EmployeeList = [];
    $scope.getEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getEmployee',
            data: {
                'PlantId': $scope.selectedData.PlantId,
                'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
                'EmployeeCategoryId': $scope.selectedData.EmployeeCategoryId,
            },
        }).then(function success(resp) {
            $scope.EmployeeList = resp.data;
        });
    }

    $scope.selectEmpDetail = function () {
        $scope.EmployeeIds = [];
        $scope.SelEmpList = [];
        for (var i = 0; i < $scope.EmployeeList.length; i++) {
            
            if ($scope.EmployeeList[i].isSelected == true) {
                $scope.SelEmpList.push($scope.EmployeeList[i]);
            }
        }

        if ($scope.SelEmpList.length > $scope.selectedData.VacancyList) {
            ShowResult('Selected Greater than vacancy allowed', 'failure');
            throw ('Invalid Request');
        }
        else {
            angular.element(document.querySelector('#EmpPop')).modal('hide');
        }
       
        $scope.getSelected();
    }

    $scope.EmpList = [];
    $scope.getSelected = function () {
        $scope.EmpList = $scope.SelEmpList;
         
    }


    // TAB - 2
    // ALL POP UPs

    // POP OPEN
    $scope.selectEmployee = function () {

        angular.element(document.querySelector('#EmployeePop')).modal('show');
    }

    $scope.openEmpCategoryPopup = function () {

        angular.element(document.querySelector('#EmpCategoryPop')).modal('show');
    }

    // POP CLOSED
    $scope.closeEmpPopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
    }
    // Select Emp
    $scope.EmployeeSelectedName = null;
    $scope.SelectedEmployeeId = null;
    $scope.selEmp = function (e) {
        $scope.SelectedEmployeeId = e.data.SystemId;
        $scope.EmployeeId = e.data.EmployeeId;
        $scope.SelEmployeeInfoList = e.data;
        $scope.Employee = e.data.EmployeeName;
        
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
        
       
    }

    $scope.EmployeeList = [];
    $scope.getAllEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getAllEmployee',
            data: { 'EmpCategoryId': $scope.EmpCategoryId},
        }).then(function success(resp) {
            $scope.EmployeeList = resp.data;
        })
    }
    //$scope.getAllEmployee();

    $scope.openEmpCategoryPopup = function () {

        angular.element(document.querySelector('#EmpCategoryPop')).modal('show');
    }

    $scope.EmployeeCategoryList = [];
    $scope.getEmployeeCategory = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getEmployeeCategory",
            //data: { 'EmpId': $scope.SelectedEmployeeId},
            dataType: 'JSON',
        }).then(function successcallback(response) {
            $scope.EmployeeCategoryList = response.data;
            
        })
    }
    $scope.getEmployeeCategory();

    $scope.EmpCategoryId = null;
    $scope.EmpCategoryName = null;
    $scope.selEmployeeCategory = function (e) {
        $scope.EmpCategoryId = e.data.Id;
        $scope.EmpCategoryName = e.data.UserName;
        angular.element(document.querySelector('#EmpCategoryPop')).modal('hide');
      //  $scope.getAllEmployee();
    }


    $scope.ResidenceMasterList = [];
    $scope.getResidenceMaster = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getResidenceMaster',

        }).then(function success(resp) {
            $scope.ResidenceMasterList = resp.data;
        })
    }

    

    // Data Saved
    $scope.selectedDataR = {
        Id: null,
        isOccupied:false,
    };
    $scope.ResidenceData = Object.assign({}, $scope.selectedDataR);

    $scope.save = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'save',
            data: {
                'data': $scope.ResidenceData,
                'EmployeeId': $scope.SelectedEmployeeId,
                'ResidenceMasterId': $scope.selResidenceMasterId,
            },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        });
    }

    $scope.ResidenceStatusLocationList = [];
    $scope.getResidenceStatusLocation = function () {
        $http({
            method: "POST",
            url: $scope.path + "getResidenceStatusLocation",
            data: {                
                'EmployeeId': $scope.SelectedEmployeeId,
                'ResidenceMasterId': $scope.selResidenceMasterId,
            },
        }).then(function seccessCallback(response) {
            $scope.ResidenceStatusLocationList = response.data
        })
            
    }


    //-----------------------------------------------------------------------------------

    function openModal() {
        $('.confirm-delete').addClass('hide');
        $('#myModal .modal-header, .modal-footer, .modal-body').removeClass('hide');
        $('#myModal').modal('show');
    }
//-----------------------------------------------------------------------------------
}