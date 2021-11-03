'use strict';
HSNTaxPercentageControllerNew.$inject = ["addressService", 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http'];
function HSNTaxPercentageControllerNew(addressService, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http) {
    $rootScope.title = "HSNTaxPercentage";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.hSNTaxPercentages = [];
    $scope.path = 'Setups/hsntaxpercentageNew/';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getgriddataUrl = $scope.path + 'GetTaxPercentage';
    $scope.gettaxcategoriesurl = $scope.path + 'GetTaxCategories';

    $scope.savegriddataUrl = $scope.path + 'Save';
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    baseService.init($scope.getListUrl, null, null, null, 'HSNCode', 'HSNCode');


    $scope.hSNTaxPercentage = {
        Id: null,
        CountryId: null,
        HSNCodeId: null,
        TaxCategoryId: null,
        Percentage: null,
        EffectiveDate: null
    };

    $scope.searchByList = [
        {
            'name': 'HSN Code',
            'value': 'HSNCode'
        },
        {
            'name': 'Tax Category',
            'value': 'TaxCategory'
        }
    ];

    $scope.SpecialTaxCollection = [];
    $scope.SpecialTax = "Normal Tax";
    $scope.getAllSpecialTax = function getAllSpecialTax() {

        $http({
            method: 'POST',
            url: $scope.path + "GetSpecialTaxList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.SpecialTaxCollection = response.data;
        });
    }
    $scope.getAllSpecialTax();

    addressService.getCountryCbo(function (result) {
        $scope.countryList = result;
    });
    $scope.taxCategoryList = [];
    //$scope.getTaxCategoryCbo = function () {
    //    cboService.getTaxCategoryCboByCountry($scope.hSNTaxPercentage.CountryId, function (result) {
    //       
    //    });
    //};

    $scope.getTaxCategoryCbo = function () {
        $http({
            method: 'POST',
            url: $scope.gettaxcategoriesurl,
            data: { 'countryid': $scope.hSNTaxPercentage.CountryId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.taxCategoryList = response.data;

        });
    };


    $scope.getHSNData = function () {
        try {
            baseService.setCurrentPage('hsnList');
            $scope.getHSNCode = function (pageno) {
                baseService.paginationBase('Setups/hsntaxpercentage/gethnslist?countryId=' + $scope.hSNTaxPercentage.CountryId, pageno, $scope.HSNParameters)
                    .then(function (result) {
                        $scope.hsnList = result.Rows;
                        $scope.HSNParameters.total_count = result.Total;
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            $scope.getHSNCode();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    function CheckField(fieldValue, fieldName) {
        try {
            if (fieldValue === null || fieldValue === '') {
                throw ('[' + fieldName + '] is required...');
            }
        } catch (e) {
            throw e;
        }
    }

    $rootScope.tempList = [];
    $scope.panelshow = false;
    $scope.panelsaveshow = false;
    $scope.hSNCodeSelectedList = [];
    $scope.hSNCodeList = [];
    $scope.recorddoubleclick = function (args) {

        var gridObj = $("#Grid").data("ejGrid");
        //getting corresponding record             
        gridObj.startEdit(gridObj.getSelectedRecords()[0]);
        //if (!$rootScope.isCollapsed) {
        //    $rootScope.toggle();
        //}
    }

    $scope.actionComplete = function actionComplete(args) {

        //if (args.requestType == "beginedit" || args.requestType == "add") {

        $("#datepick").ejDatePicker();

        //}
    }
    function datepickerchange(args) {


    }
    $scope.refreshTemplate = function (args) {

        $($("#Grid .griddatepicker")[args.rowIndex]).ejDatePicker({ dateFormat: "dd-MMM-yyyy", value: args.data.EffectiveDate, "change": datepickerchange });

    }

    $scope.openDialog = function (args) {

        try {
            $scope.paramvisible = true;
            $scope.panelsaveshow = false;
            $scope.hSNTaxPercentage.EffectiveDate = "";

            if (angular.isUndefinedOrNull($scope.hSNTaxPercentage.CountryId) == true)
                throw "Please select country";

            var target = $('#selecttax').data("ejListBox");
            var checkedItems = target.getCheckedItems();

            if (checkedItems.length === 0)
                throw "Please select tax group";

            if (args == "edit") {
                $scope.paramvisible = false;
                $scope.getupdatedata(args);
            }


            angular.element(document.querySelector('#recipeMaterialPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }

    }
    angular.isUndefinedOrNull = function (val) {
        return angular.isUndefined(val) || val === null || val === ""
    }
    $scope.paramvisible = false;

    $scope.datasource = [];
    $scope.datasourcecolumns = [];
    $scope.selectedtax = [];
    $scope.getupdatedata = function (args) {
        try {

            if (angular.isUndefinedOrNull($scope.hSNTaxPercentage.CountryId) == true)
                throw "Please select country";


            var target = $('#selecttax').data("ejListBox");
            var checkedItems = target.getCheckedItems();

            if (checkedItems.length === 0)
                throw "Please select tax group";

            if (args == "add") {
                if (angular.isUndefinedOrNull($scope.hSNTaxPercentage.EffectiveDate) == true)
                    throw "Please select date";


            }

            var taxes = [];
            for (var i = 0; i < checkedItems.length; i++) {
                taxes.push(checkedItems[i].data.Id);
            }


            $scope.panelsaveshow = false;
            $http({
                method: 'POST',
                url: $scope.getgriddataUrl,
                data: {
                    'countryid': $scope.hSNTaxPercentage.CountryId, 'taxcodes': taxes, 'effectivedate': $scope.hSNTaxPercentage.EffectiveDate, 'SpecialTaxID': $scope.SpecialTax
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                    $scope.hSNCodeSelectedList = [];
                }
                else {

                    $scope.datasource = response.data.DATA;
                    $scope.datasourcecolumns = response.data.COLUMNS;

                    var fieldList = [];
                    for (var i = 0; i < $scope.datasourcecolumns.length; i++) {
                        if ($scope.datasourcecolumns[i] == "HSNCode")
                            if ($scope.SpecialTax != "Normal Tax")
                                fieldList.push({ field: $scope.datasourcecolumns[i],headerText:'Tax Code', allowEditing: false, visible: true });
                            else
                                fieldList.push({ field: $scope.datasourcecolumns[i], allowEditing: false, visible: true });
                        else if ($scope.datasourcecolumns[i] == "PK")
                            fieldList.push({ field: $scope.datasourcecolumns[i], allowEditing: false, isPrimaryKey: true, visible: false });
                        else if ($scope.datasourcecolumns[i].includes("ID") || $scope.datasourcecolumns[i].includes("PK"))
                            fieldList.push({ field: $scope.datasourcecolumns[i], allowEditing: false, visible: false });
                        else if ($scope.datasourcecolumns[i].includes("Code") || $scope.datasourcecolumns[i].includes("Date"))
                            fieldList.push({ field: $scope.datasourcecolumns[i], allowEditing: false, visible: true });
                        else
                            fieldList.push({ field: $scope.datasourcecolumns[i], format: "{0:N2}", allowEditing: true, visible: true, editType: ej.Grid.EditingType.Numeric, editParams: { decimalPlaces: 2 } });
                    }


                    $('#Grid').ejGrid({
                        dataSource: response.data.DATA,
                        allowPaging: true,
                        columns: fieldList,
                        allowFiltering: true,
                        endEdit: $scope.savesingledata,
                        allowKeyboardNavigation: true,
                        filterSettings: { filterType: "excel" },
                        editSettings: { allowEditing: true, editMode: "dialog", }

                    });

                    $scope.panelsaveshow = true;
                }
            });
        } catch (e) {
            ShowResult(e, 'failure', 'recipeMaterialPopUp');
        }
    }


    $scope.savesingledata = function (args) {

        var datatosave = [];
        datatosave.push(args.data);
        var gridObj = $("#Grid").data("ejGrid");
        var data = gridObj.model.dataSource;
        $http({
            method: 'POST',
            url: $scope.savegriddataUrl,
            data: { 'data': datatosave, 'taxtype': $scope.SpecialTax },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');

            }
            else {
                ShowResult(response.data.Message, 'success', 'recipeMaterialPopUp');
                // $scope.getupdatedata();
            }
        });
    }
    $scope.savetaxdata = function () {

        var gridObj = $("#Grid").data("ejGrid");
        var data = gridObj.model.dataSource;
        $.ajax({
            method: 'POST',
            url: $scope.savegriddataUrl,
            data: { 'data': data },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');

            }
            else {
                ShowResult(response.data.Message, 'success', 'recipeMaterialPopUp');

            }
        });
    }


    $scope.Print = function () {

        var gridObj = $("#Grid").data("ejGrid");
        var data = gridObj.model.dataSource;
        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: { 'data': data }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');


            }
            else {

                window.location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
            }
        });
    }
    $scope.Clear = function () {

        try {
            if ($scope.datasource.length > 0) {
                $scope.datasource = [];
                $('#Grid').ejGrid({
                    dataSource: $scope.datasource,
                    allowPaging: false,
                    allowFiltering: false,
                    allowKeyboardNavigation: false,

                });
            }

        } catch (e) {

        }



    }
    $scope.Back = function () {
        angular.element(document.querySelector('#recipeMaterialPopUp')).modal('hide');
    }
}