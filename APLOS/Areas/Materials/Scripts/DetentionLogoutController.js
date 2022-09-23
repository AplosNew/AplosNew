'use strict';
DetentionLogoutController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function DetentionLogoutController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Detention Logout";
    $scope.Action = 'Save';
    $scope.path = 'Materials/DetentionLogout/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getStorage = $scope.path + 'StorageSql';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'Delete';

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;


    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // All List Declaration
    $scope.DetentionLogGridList = [];

    // Get Detention Log Grid
    $scope.getDetentionLogGrid = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getDetentionLogGrid',
        }).then(function successCallback(response) {
            $scope.DetentionLogGridList = response.data;
        })
    }
    $scope.getDetentionLogGrid();



    // #region Detention Log update
    var LogTime = new Date();
    $scope.ModalTemp = {
        Id: null,
        DetentionTypeId: null,
        WorkCenterId: null,
        CellPhnNo: null,
        IssueByNo: null,
        Remarks: null,
        LogTime: LogTime,
        EmployeeName:null,
    };
    $scope.ModalNew = Object.assign({}, $scope.ModalTemp);


    //-------------------------------------------------------------------

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
            url: 'Materials/DetentionLog/GetDetentionResponsible',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ResponsibleList = resp.data;

            
        });
    }


    //$scope.ResponsiblePersonId = null;
    //$scope.ResponsiblePersonName = null;

    //$scope.doubleResponsible = function (e) {
    //    $scope.ResponsiblePersonId = e.data.ResponsiblePersonId;
    //    $scope.ResponsiblePersonName = e.data.ResponsiblePerson;
    //    angular.element(document.querySelector('#ResponiblePersonPop')).modal('hide');
    //    //$scope.getRespPersonContactNo();
    //}

    $scope.closeResponsiblePopUp = function () {
        angular.element(document.querySelector('#ResponiblePersonPop')).modal('hide');
    }


    $scope.DetentionTypeList = [];
    $scope.getDetentionType = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionLog/getDetentionTypeListByDepartment'
        }).then(function successCallback(response) {
            $scope.DetentionTypeList = response.data;

        });
    }
    $scope.getDetentionType();

    $scope.WorkCenterList = [];
    $scope.getWorkCenter = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionLog/GetWorkCenter',
        }).then(function successCallback(response) {
            $scope.WorkCenterList = response.data;
        })
    }
    $scope.getWorkCenter();

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


    $scope.Get = function (args) {

        $scope.ModalNew = Object.assign({}, args.data);
        $scope.ModalNew.EmployeeName = args.data.EmployeeName;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
           
        }
    };

    $scope.Save = function () {
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
                ShowResult(response.data.Message, 'success');

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }
    // #endregion Detention Log update


   
}
//-----------------------------------------------------------------------------------

function openModal() {
    $('.confirm-delete').addClass('hide');
    $('#myModal .modal-header, .modal-footer, .modal-body').removeClass('hide');
    $('#myModal').modal('show');
}
//-----------------------------------------------------------------------------------