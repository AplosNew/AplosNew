'use strict';
EmployeeProfileApprovalController.$inject = ['addressService', 'fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function EmployeeProfileApprovalController(addressService, fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter,$window) {
    $rootScope.title = 'Employee Profile Approval';
    $scope.Action = 'Save';      
    $scope.path = 'employees/EmployeeProfileApproval/';
    $scope.getListUrl = $scope.path + 'GetUnApprovedEmployeeList';
    $scope.saveApprovedUrl = $scope.path + 'SaveApprovedEmployee';



    $scope.employees = [];


    $scope.LoadEmployeeDataForGrid = function () {
        try {
            
            $http({
                method: 'GET',
                url: $scope.getListUrl ,
                //data: JSON.stringify(data),
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.ShowResultCustom(response.Message, 'failure');
                }
                else {
                    $scope.employees = null;
                    $scope.employees = response.data;


                }
            }), function errorCallBack(response) {
                $scope.ShowResultCustom(response.Message, 'failure');
            };




        } catch (e) {
            $scope.ShowResultCustom(e, "failure");
        }
    };


    $scope.LoadEmployeeDataForGrid();
    $window.onresize = function (event) {

        $scope.actionCompleteSelected();
     

    };
    $scope.actionCompleteSelected = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#Grid").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container
              
                $("#Grid").children('.e-grid.e-headercell').css('height', '100px');
                //args.requestType: "filtering"
                //var filtereddata = gridObj.getFilteredRecords();
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };






   
  
   
    // #region checkbox all

    angular.isUndefinedOrNull = function (val) {
        return angular.isUndefined(val) || val === null || val === "";
    };
    function checkChangeemployee(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.employees, { 'SystemID': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState === "check")
                row[0].CheckBoxSelect = true;
            else
                row[0].CheckBoxSelect = false;
        }

    }
    function headCheckChangeemployee(e) {
        if (e.model.checkState === "check") {

            // var gridObj = $("#Gridemployee").data("ejGrid");
            var filtered = $("#Grid").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length === 0) {
                for (var i = 0; i < $scope.employees.length; i++) {
                    $scope.employees[i].CheckBoxSelect = true;
                }
            }
            else {
                for (var i = 0; i < $scope.employees.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.employees[i].SystemID === filtered[j].SystemID)
                            $scope.employees[i].CheckBoxSelect = true;
                    }

                }
            }

            var checkbox = $("#Grid .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        else {
            var filtered = $("#Grid").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.employees.length; i++) {
                    $scope.employees[i].CheckBoxSelect = false;
                }
            }
            else {
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.employees[i].SystemID == filtered[j].SystemID)
                            $scope.employees[i].CheckBoxSelect = false;
                    }

                }
            }
            var checkbox = $("#Grid .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        //header level check
    }
    $scope.dataBoundemployee = function (args) {
        $("#Grid .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });

    }
    $scope.refreshTemplateemployee = function (args) {
        if (args.rowIndex == 0) {
            $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });
        }

        var valobj = $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.employees, { 'SystemID': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].CheckBoxSelect == true)
                $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee });
    }





    // #endregion


    $scope.messageText = "";



    $scope.SaveData = function () {

        $.ajax({
            type: "POST",
            url: $scope.saveApprovedUrl,
            data:
            {
                'employeeInformation': $scope.employees
               
            },
            dataType: "json",
            success: function (data) {
                $scope.ShowResultCustom(data.Message, "success");
                $scope.LoadEmployeeDataForGrid();
            }

        });
        //$http({
        //    method: 'POST',
        //    url: $scope.saveApprovedUrl,
        //    data: $scope.employees,
        //    dataType: 'JSON'
        //}).then(function successCallback(response) { });

    };


    $scope.ShowResultCustom = function (message, type) {
        $("#dialogMessage").ejDialog("setTitle", "Success");
        $scope.messageText = message;
        $scope.messageTitle = "Message";

        if (type === "failure")
            $("#dialogMessage").ejDialog("setTitle", "Error");

        var eDialog = $("#dialogMessage").data("ejDialog");
        eDialog.open();

    };

    
}
