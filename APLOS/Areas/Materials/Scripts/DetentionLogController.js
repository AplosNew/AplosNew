'use strict';
DetentionLogController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function DetentionLogController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Detention Log";
    $scope.Action = 'Save';
    $scope.path = 'Materials/DetentionLog/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getStorage = $scope.path + 'StorageSql';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'Delete';

    var LogTime = new Date();
    $scope.ModalTemp = {
        Id: null,
        DetentionTypeId: null,  
        DepartmentId:null,
        WorkCenterId: null,
        CellPhnNo: null,
        IssueByNo: null,
        Remarks: null,
        LoginTime: LogTime,
        isClose: false,
        isUpdate: 0,
        MachineMasterId:null
    };
    $scope.ModalNew = Object.assign({}, $scope.ModalTemp);

   
    // Responsible Person
    $scope.openEmployeePopUp = function () {
        $scope.getsR();
        angular.element(document.querySelector('#ResponiblePersonPop')).modal('show');
    }
    $scope.ResponsibleList = [];
    $scope.userResponsiblePersonList = [];
    $scope.getsR = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionLog/GetDetentionResponsible?detentionId=' + $scope.ModalNew.DetentionId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ResponsibleList = resp.data;

            for (var i = 0; i < $scope.userResponsiblePersonList.length; i++) {
                for (var j = 0; j < $scope.ResponsibleList.length; j++) {
                    if ($scope.userResponsiblePersonList[i].Id === $scope.ResponsibleList[j].Id) {
                        $scope.ResponsibleList[j].chk = true;
                    }
                }
            }
        });
    }

    //-------------------------------------------------------------------------
    $scope.chkdResponsiblePersonList = [];
    $scope.ResponsiblePersonGridAllCheck = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAll });
    };

    function CheckBoxSelectAll(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        for (var i = 0; i < $scope.ResponsibleList.length; i++) {
            $scope.ResponsibleList[i].chk = ChkOrUnchk;
            $scope.chkdResponsiblePersonList = $scope.ResponsibleList[i].chk;
        }

        var gridObj = $("#GridResponsible").data("ejGrid");
        gridObj.refreshContent();
    };



    function checkResponsiblePersonExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === Id) {
                return true;
            }
        }
        return false;
    }

    
    $scope.SendResponsiblePerson = function () {        
            if (baseService.arrayLength($scope.ResponsibleList) > 0) {
                angular.forEach($scope.ResponsibleList, function (a) {
                    //if (checkResponsiblePersonExist($scope.userResponsiblePersonList, a.Id) === false) {
                        if (a.chk) {
                            var ob = {};
                            ob.Id = null;
                            ob.ResponsiblePersonId = a.ResponsiblePersonId; 
                            ob.EmployeeCode = a.EmployeeCode;
                            ob.EmployeeName = a.ResponsiblePerson;
                            ob.CellPhnNo = a.CellPhnNo;
                            ob.Department = a.Department;
                            ob.Section = a.Section;
                            ob.SubSection = a.SubSection;
                            ob.LegalDesignation = a.LegalDesignation;
                            ob.isActive = a.isActive;
                            $scope.userResponsiblePersonList.push(ob);
                            ob = {};
                        }
                    //}

                });
            }

            $scope.$broadcast('show-errors-check-validity');

           
       
        $scope.closeResponsiblePopUp();
    };
    //-------------------------------------------------------------------------


    //$scope.ResponsiblePersonId = null;
    //$scope.ResponsiblePersonName = null;
    //$scope.doubleResponsible = function (e) {
    //    $scope.ResponsiblePersonId = e.data.ResponsiblePersonId;
    //    $scope.ResponsiblePersonName = e.data.ResponsiblePerson;
    //    angular.element(document.querySelector('#ResponiblePersonPop')).modal('hide');
    //    $scope.getRespPersonContactNo();
    //}

    $scope.closeResponsiblePopUp = function () {
        angular.element(document.querySelector('#ResponiblePersonPop')).modal('hide');
    }


    // Detention Type By Department

    $scope.DetentionTypeList = [];
    $scope.getDetentionTypeListByDepartment = function (departmentid) {
        $http({
            method: 'POST',
            url: 'Materials/DetentionLog/getDetentionTypeListByDepartment?departmentid=' + departmentid
        }).then(function successCallback(response) {
            $scope.DetentionTypeList = response.data;
           
        });
    }
    $scope.getDetentionTypeListByDepartment();

    $scope.ProcesssList = [];
    $scope.getProcessList = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionLog/getProcessList'
        }).then(function successCallback(response) {
            $scope.ProcesssList = response.data;
           
        });
    }
    $scope.getProcessList();

    $scope.WorkCenterList = [];
    $scope.WorkCenter = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionLog/GetWorkCenter',
        }).then(function successCallback(response) {
            $scope.WorkCenterList = response.data;

        });
    }
    $scope.WorkCenter();
    // Get Workcenter by pressing on key suggest
    
   

    // Get Workcenter by pressing on key suggest end

    

    $scope.getRespPersonContactNo = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionLog/getRespPersonContactNo?ResponsiblePersonId=' + $scope.ResponsiblePersonId,
        }).then(function successCallback(response) {
            $scope.ModalNew.CellPhnNo = response.data[0].CellPhnNo;
        })
    }

    $scope.getIssueByNo = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionLog/getIssueByNo',
        }).then(function successCallback(response) {
            $scope.ModalNew.IssueByNo = response.data[0].IssueByNo;
        })
    }
    $scope.getIssueByNo();

    $scope.DepartmentList = [];
    $scope.GetDepartment = function () {
        $http.get('Materials/DetentionLog/GetDepartment')
            .then(
                function successCallback(response) {
                    $scope.DepartmentList = response.data;
                }
        )
    }
    $scope.GetDepartment();

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        $http({
            method: 'POST',
            url: 'Materials/DetentionLog/Save',
            data: {
                'data': $scope.ModalNew,
                'ResponsiblePersonId': $scope.ResponsiblePersonId,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.ModalNew.Id = response.data.Data.Id;
                $scope.SaveResponsiblePerson();
                ShowResult(response.data.Message, 'success');
                $scope.Clear();
                
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }


    //-------------------------------------------------------------------------
    $scope.chkdResponsiblePersonList = [];
    $scope.ResponsiblePersonGridAllCheck = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAll });
    };

    function CheckBoxSelectAll(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        for (var i = 0; i < $scope.ResponsibleList.length; i++) {
            $scope.ResponsibleList[i].chk = ChkOrUnchk;
            $scope.chkdResponsiblePersonList = $scope.ResponsibleList[i].chk;
        }

        var gridObj = $("#GridResponsible").data("ejGrid");
        gridObj.refreshContent();
    };

    

    function checkResponsiblePersonExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === Id) {
                return true;
            }
        }
        return false;
    }

    
    $scope.SaveResponsiblePerson = function () {
        try {

            if (baseService.arrayLength($scope.BudgetCodeList) > 0) {
                angular.forEach($scope.ResponsibleList, function (a) {
                    if (checkResponsiblePersonExist($scope.userResponsiblePersonList, a.Id) === false) {
                        if (a.chk) {
                            var ob = {};
                            ob.Id = null;
                            //ob.EmployeeCode = a.EmployeeCode;
                            //ob.EmployeeName = a.ResponsiblePerson;
                            //ob.Department = a.Department;
                            //ob.Section = a.Section;
                            //ob.SubSection = a.SubSection;
                            //ob.LegalDesignation = a.LegalDesignation;
                            ob.isActive = a.isActive
                            $scope.userResponsiblePersonList.push(ob);
                            ob = {};
                        }
                    }

                });
            }

            $scope.$broadcast('show-errors-check-validity');

            $http({
                method: 'POST',
                url: $scope.path + 'saveDtentionLogResPerson',
                data: {
                    'data': $scope.userResponsiblePersonList,
                    'detentionLogId': $scope.ModalNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');


                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
        catch (ex) {
            ShowResult(ex, 'failure');
        }
        $scope.closeResponsiblePopUp();
    };
    //-------------------------------------------------------------------------


    $scope.Clear = function () {
        ClearFields();
        return true;
    };


    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModalNew = {
            Id: null,
            DetentionTypeId: null,
            WorkCenterId: null,           
            Remarks: null,
            
        };
        $scope.getIssueByNo();
        $scope.ModalNew = Object.assign({}, $scope.ModalTemp);
        $scope.userResponsiblePersonList = [];
        
        //ob = {};
    }

    $scope.saveDtentionLogResPerson = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'saveDtentionLogResPerson',
            data: {
                'data': $scope.userResponsiblePersonList,
                'detentionLogId': $scope.ModalNew.Id
            },
            dataType:'JSON'
        }).then(function successCallback(response) {

        })
    }

    // Get Machine Master Asset
    $scope.MachineMasterAssetList = [];
    $scope.getMachineMasterAsset = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getMachineMasterAsset',
            dataType:'JSON',
        }).then(function successCallback(response) {
            $scope.MachineMasterAssetList = response.data;
        });
    }
    $scope.getMachineMasterAsset();
    //----------------------------------------------------
    
    function currentTime() {
        let date = new Date();
        let hh = date.getHours();
        let mm = date.getMinutes();
        let ss = date.getSeconds();
        let session = "AM";

        if (hh == 0) {
            hh = 12;
        }
        if (hh > 12) {
            hh = hh - 12;
            session = "PM";
        }

        hh = (hh < 10) ? "0" + hh : hh;
        mm = (mm < 10) ? "0" + mm : mm;
        ss = (ss < 10) ? "0" + ss : ss;

        let time = hh + ":" + mm + ":" + ss + " " + session;

        document.getElementById("clock").innerText = time;
        let t = setTimeout(function () { currentTime() }, 1000);
    }
    //currentTime();
    //----------------------------------------------------
}