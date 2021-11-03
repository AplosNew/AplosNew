'use strict';
notificationURLController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$filter", "$window", "$http", "$controller"];
function notificationURLController(cboService, commonMessage, $scope, $rootScope, baseService, $filter, $window, $http, $controller) {

    $scope.path = 'Modules/NotificationURL/';
    $scope.currencyList = [];
    $scope.Action = 'Save';
    //search
    $scope.model = {
        SystemId : null,
        CompanyGroupId: null,
        URL: null
    };

    $scope.notificationURLList = [];

    $scope.modelFilterByList = [   
        { value: 'CompanyGroupId', name: 'CompanyGroupId' },
        { value: 'URL', name: 'URL ' }   
    ];
    $scope.EmpAdvanceReqList = [];
    $scope.companyGroupList = [];

    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });

    $scope.CompanyGroupId = null;


    $scope.searchCol = "SystemId";
    $scope.searchVal = "";
    $scope.getData = function () {

        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.path + "NotificationURLGetList?column=" + $scope.searchCol + "&value=" + $scope.searchVal
        }).then(function successCallback(response) {
            $scope.notificationURLList= response.data;
        });
    };
   $scope.getData();  

  

    $scope.Get = function (args) {
        $scope.model = Object.assign({}, $scope.modelMain);
        $scope.LoadData(args.data.SystemId);
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }

    };
    $scope.LoadData = function (Id) {
        $http({
            method: 'POST',
            url: $scope.path + "Get?SystemId=" + Id
        }).then(function successCallback(response) {
            $scope.model = response.data.master[0];
            $scope.Action = 'Update';
        });
    };

    function CheckField(fieldValue, fieldName) {
        try {
            if (fieldValue === null || fieldValue === '' || fieldValue === undefined) {
                throw '[' + fieldName + '] is required...';
            }
        } catch (e) {
            throw e;
        }
    }

    function ValidationMaster() {
        try {
            

            //if (new Date($scope.model.RequisitionAddedDate) > new Date($scope.model.RequisitionRequiredDate)) {
            //    throw "Required Date cann't less than Entry Date.";
            //}

            CheckField($scope.model.CompanyGroupId, "Company Group");
            CheckField($scope.model.URL, "URL");
          
        } catch (e) {
            throw e;
        }
    }

    $scope.Save = function () {
        try {
            ValidationMaster();

            var DropDownListcheckedBy = $("#ddlcheckedByList").data("ejDropDownList");
            //var CheckedBy = DropDownListcheckedBy.getSelectedValue();
            //$scope.model.CheckedBy = CheckedBy;

            $http({
                method: 'POST',
                data: { NotificationURLlist: $scope.model },
                url: "Modules/NotificationURL/NotificationURLSave"
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');

                    $scope.Cancel();
                    $scope.getData();
                    $scope.Action = 'Save';
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Delete = function () {
        try {
            $http({
                method: 'GET',
                url: "Modules/NotificationURL/NotficationURLDelete?SystemId=" + $scope.model.SystemId
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                    $scope.Cancel();
                    $scope.getData();
                    $scope.Action = 'Save';
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Cancel = function () {
        $scope.Action = 'Save';
        
        $("#notificationURLList").ejGrid("instance").refreshContent();
        $scope.modelMain = {
            SystemId: "",
            CompanyGroupId: null,
            URL: null
            
        };
        $scope.model = Object.assign({}, $scope.modelMain);
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

}