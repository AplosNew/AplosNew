'use strict';
MedicalLogController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$interval'];
function MedicalLogController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $interval) {
    $rootScope.title = 'Medical Log';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/MedicalLog/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.MedicinePurposeUrl = 'HumanResource/MedicineMaster/getMedicinePurpose';
    $scope.MedicineMasterUrl = 'HumanResource/MedicineReceipt/getMedicineData';
    $scope.EmployeeUrl = 'HumanResource/MedicalLog/getEmployee'
    $scope.deleteUrl = $scope.path + 'Delete/';
    baseService.init($scope.getListUrl);
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.getSeqUrl = $scope.path + 'CountEmployeeVisiting';

    $scope.CountNoOfVisits = [];
    $scope.CountEmployeeVisiting = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'CountEmpVisits',
            data: { 'empsystemCode': $scope.ModalNew.EmployeeSysId},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModalNew.NoOfVisits = response.data[0].NoOfVisits;
            $scope.ModalNew.NoOfVisits++;
        });
    };

    // #region POP UP 
    $scope.openEmpPopUp = function () {
        angular.element(document.querySelector('#empPopUpId')).modal('show');

    }

    $scope.closeEmpPopUp = function () {

        angular.element(document.querySelector('#empPopUpId')).modal('hide');

    }

    $scope.openMedicinePopUp = function () {
        angular.element(document.querySelector('#medicinePopUp')).modal('show');
        $scope.getMedicineData();
    }

    $scope.closeMedicinePopUp = function () {

        angular.element(document.querySelector('#medicinePopUp')).modal('hide');

    }

    $scope.openSicknessPopUp = function () {
        angular.element(document.querySelector('#sicknessPopUpid')).modal('show');
        $scope.getSickness();
    }

    $scope.closeSicknessPopUp = function () {

        angular.element(document.querySelector('#sicknessPopUpid')).modal('hide');

    }
    // #endregion POP UP 
    

    // TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.redirectTab = function () {
        if ($scope.tabForm1.$invalid) {
            $scope.setTab(1);
        }
        else if ($scope.tabForm2.$invalid) {
            $scope.setTab(2);
        }
       
    };

    // #ENDREGION

    var todaysDate = new Date();
    var curTime = todaysDate ;
    
    $scope.getTime = function () {
        var now = new Date();
        curTime = now.getHours() + ": " + now.getMinutes() + ": " + now.getSeconds();
    }
    $interval($scope.getTime, 1000); 
    // Form Objects
    $scope.ModelTemp = {
        Id: null,
        Date: todaysDate,
        Time: curTime,
        NoOfVisits:0,
        Remarks: null,
        EmployeeName: null,
        EmployeeSysId:null
    };
    $scope.ModalNew = Object.assign({}, $scope.ModelTemp);

    // #region Get Sickness list and send in child grid
    $scope.MedicinePurposeList = [];
    $scope.UserSicknessList = [];
    $scope.getSickness = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getSicknessType',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MedicinePurposeList = response.data;

            for (var i = 0; i < $scope.UserSicknessList.length; i++) {
                for (var j = 0; j < $scope.MedicinePurposeList.length; j++) {
                    if ($scope.UserSicknessList[i].Id === $scope.MedicinePurposeList[j].Id) {
                        $scope.MedicinePurposeList[j].chk = true;
                    }
                }
            }

        });
    }
    

    $scope.SendSickness = function () {
        if (baseService.arrayLength($scope.MedicinePurposeList) > 0) {
            angular.forEach($scope.MedicinePurposeList, function (a) {

                if (a.chk) {
                    var ob = {};
                    ob.Id = a.Id;
                    ob.Sickness = a.Sickness;
                    ob.Category = a.Category;
                    
                    $scope.UserSicknessList.push(ob);
                    ob = {};
                    a.chk = false;
                }

            });
        }
        
    };
    // #endregion Get Sickness list and send in child grid

    // #region Get Medicine list and send in child grid
    $scope.MedicineList = [];
    $scope.userMedicineList = [];
    $scope.getMedicineData = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getMedicineList',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MedicineList = response.data;

            for (var i = 0; i < $scope.userMedicineList.length; i++) {
                for (var j = 0; j < $scope.MedicineList.length; j++) {
                    if ($scope.userMedicineList[i].Id === $scope.MedicineList[j].Id) {
                        $scope.MedicineList[j].chk = true;
                    }
                }
            }
        });
    }
    

    $scope.SendMedicine = function () {
        if (baseService.arrayLength($scope.MedicineList) > 0) {
            angular.forEach($scope.MedicineList, function (a) {

                if (a.chk) {
                    var ob = {};
                    ob.Id = a.Id;
                    ob.Code = a.Code;
                    ob.Medicine = a.Medicine;
                    ob.Category = a.Category;
                    ob.SubCategory = a.SubCategory;
                    $scope.userMedicineList.push(ob);
                    ob = {};
                }

            });
        }
        
    };
    // #endregion Get Medicine list and send in child grid

    // #region Get All Employee and select by double click
    $scope.EmployeeList = [];
    $scope.getEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.EmployeeUrl,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EmployeeList = resp.data;
        });

    }
    $scope.getEmployee();
    
    $scope.doubleEmployee = function (e) {
        $scope.ModalNew.EmployeeSysId = e.data.SystemId;
        $scope.ModalNew.EmployeeName = e.data.EmployeeName;
        $scope.closeEmpPopUp();
        $scope.CountEmployeeVisiting();
        
    }
    // #endregion Get All Employee and select by double click

    //=======================================SAVE============================================

    $scope.Save = function () {
       
        $scope.$broadcast('show-errors-check-validity');

        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: {
                'data': $scope.ModalNew,
                'empSystemId': $scope.ModalNew.EmployeeSysId,
                'medicinepurposelist': $scope.UserSicknessList,
                'medicinelist': $scope.userMedicineList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                ClearFields(response.data.Sequence);
                $scope.getData();
                $scope.Action = 'Update';

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };


    //=======================================SAVE CLOSE==========================================

    $scope.MedicalLogGridList = [];
    $scope.medicallogGridView = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'medicallogGridView',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MedicalLogGridList = response.data;
        })
    }
    $scope.medicallogGridView();


    //--------------------------------------------------------LISTS FOR SEARCH-------------------------------------------------------------------
    $scope.searchByMedicine = "UserName"; $scope.searchMedicine = "";
    
    $scope.MedicineSearchByList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Category',
            'value': 'Category'
        },

    ];

    
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: 'HumanResource/MedicineReceipt/GetList',
            data: { column: $scope.searchByMedicine, value: $scope.searchMedicine },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MedicineList = response.data;
        });
    }

    $scope.searchBySickness = "UserName"; $scope.searchSickness = "";
    $scope.SicknessSearchByList = [
        {
            'name': 'UserName',
            'value': 'UserName'
        },
        {
            'name': 'Category',
            'value': 'Category'
        },
    ];

   // $scope.MedicinePurposeList = []
    $scope.getSearchSicknessData = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getSearchSicknessData',
            data: { column: $scope.searchBySickness, value: $scope.searchSickness},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MedicinePurposeList = response.data;
        });
    }
    // #region
    $scope.searchByEmployee = "EmployeeName"; $scope.searchEmployee = "";
    $scope.EmployeeSearchList = [
        {
            'name': 'EmployeeCode',
            'value': 'EmployeeCode'
        },
        {
            'name': 'EmployeeName',
            'value': 'EmployeeName'
        },
        {
            'name': 'DOJ',
            'value': 'DOJ',
        }
    ]
    $scope.getSearchedEmployee = function () {
        $http({
            method: 'POST',
            url: 'HumanResource/MedicalLog/getSearchedEmployee',
            data: { column: $scope.searchByEmployee, value: $scope.searchEmployee },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EmployeeList = response.data;
        });
    }
    // #endregion
    // Get medicines pop up screen by medicine receipt 
    $scope.MedicineMasterId = null;
    $scope.MedicineByPurposeList = [];

    $scope.openMedicineReceiptPopUp = function (e) {
        $scope.MedicineMasterId = e.data.Id;
        angular.element(document.querySelector('#medicineExpDateWisePopUp')).modal('show');
        $http({
            method: 'POST',
            url: $scope.path + 'getMedicineByReceipt',
            data: { 'medicinemasterId': e.data.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MedicineByPurposeList = response.data;

            for (var i = 0; i < $scope.UserSicknessList.length; i++) {
                for (var j = 0; j < $scope.MedicineByPurposeList.length; j++) {
                    if ($scope.UserSicknessList[i].Id === $scope.MedicineByPurposeList[j].Id) {
                        $scope.MedicineByPurposeList[j].chk = true;
                    }
                }
            }

        
        });
    }

    $scope.SendMedicine = function () {
        if (baseService.arrayLength($scope.MedicineByPurposeList) > 0) {
            angular.forEach($scope.MedicineByPurposeList, function (a) {

                if (a.chk) {
                    var ob = {};
                    ob.Id = a.Id;                  
                    ob.Medicine = a.Medicine;
                    ob.Category = a.Category;
                    ob.SubCategory = a.SubCategory;
                    ob.Quantity = a.Quantity;
                    ob.NoOfDays = a.NoOfDays;
                    ob.Remarks = a.Remarks;
                    $scope.userMedicineList.push(ob);
                    ob = {};
                    a.chk = false;
                }

            });
        }

    };

    $scope.closeMedicineReceiptPopUp = function () {
        angular.element(document.querySelector('#medicineExpDateWisePopUp')).modal('hide');
    }


    $scope.ob2 = {};
    $scope.subtractStock = function (d) {
        $scope.ob2 = d.data;
        $scope.ob2.Stock = $scope.ob2.Quantity - $scope.ob2.Stock;
        var gridObj = $("#GridEdit").data("ejGrid");
        gridObj.refreshContent();
        gridObj.refreshTemplate();
    }
}