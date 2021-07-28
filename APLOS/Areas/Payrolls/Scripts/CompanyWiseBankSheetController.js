'use strict';
CompanyWiseBankSheetController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function CompanyWiseBankSheetController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Company Wise Bank Sheet';
    $scope.path = 'Payrolls/CompanyWiseBankSheet/';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath
    $scope.GetList = [];

    //#region Defaul Selection
    $scope.year = new Date().getFullYear().toString();
    $scope.month = new Date().getMonth().toString();
    $scope.monthList = [
        {
            Value: 1,
            Text: 'January'
        },
        {
            Value: 2,
            Text: 'February'
        },
        {
            Value: 3,
            Text: 'March'
        },
        {
            Value: 4,
            Text: 'April'
        },
        {
            Value: 5,
            Text: 'May'
        },
        {
            Value: 6,
            Text: 'June'
        },
        {
            Value: 7,
            Text: 'July'
        },
        {
            Value: 8,
            Text: 'August'
        },
        {
            Value: 9,
            Text: 'September'
        },
        {
            Value: 10,
            Text: 'October'
        },
        {
            Value: 11,
            Text: 'November'
        },
        {
            Value: 12,
            Text: 'December'
        }
    ];
    $scope.year = new Date().getFullYear().toString();
    $scope.month = new Date().getMonth().toString();
    $scope.yearList = [];
    cboService.getCboLeaveYear(function (result) {
        $scope.yearList = result;
    });

    $scope.SelectDefaultValue = function (args) {
        var x = new Date();
        x.setDate(10);
        x.setMonth(x.getMonth() - 1);

        for (var i = 0; i < $scope.yearList.length; i++) {
            if ($scope.yearList[i].Text === x.getFullYear().toString()) {
                $scope.year = $scope.yearList[i].Text;
                $scope.month = (x.getMonth() + 1).toString();
                continue;
            }
        }

        //$scope.year = "2018";
        var DropDownListYear = $("#ddlYearList").data("ejDropDownList");
        DropDownListYear.selectItemByText($scope.year);

    };

    $scope.isActive = true;
    $scope.isSeperated = false;
    $scope.isMaternity = false;
    $scope.Show = false;
    //#endregion 

    $scope.getData = function () {
        try {
            var DropDownListMonth = $("#ddlMonthList").data("ejDropDownList");
            var DropDownListYear = $("#ddlYearList").data("ejDropDownList");
            $scope.month = DropDownListMonth.getSelectedValue();
            $scope.year = DropDownListYear.getSelectedValue();
            $http({
                method: 'POST',
                url: $scope.path + "GetData",
                data: {
                    'month': $scope.month,
                    'year': $scope.year,
                    'isActive': $scope.isActive,
                    'isSeperated': $scope.isSeperated,
                    'isMaternity': $scope.isMaternity,
                }
            }).then(function successCallback(response) {
                $scope.Show = true;
                $scope.GetList = response.data;
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.CheckTable = function () {
        $scope.Show = false;
    }

    var getString = function (data, column) {
        var kk = "";
        var collection = [];
        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) === false) {
                if (kk === "") {
                    kk += "'" + data[i][column] + "'";
                }
                else {
                    kk += ",'" + data[i][column] + "'";
                }

                collection.push(data[i][column]);
            }
        }
        return kk;
    };

    $scope.GetReport = function () {
        try {
            $scope.fileName = "CompanyWiseBankReport.xls";
            var parameters = [];
            var gridObj = $("#empInfoGrid").data("ejGrid");
            var filteredRecords = gridObj.getFilteredRecords();
            if (filteredRecords.length == 0) {
                filteredRecords = $scope.GetList;
            }

            parameters.push({ "Key": "PaymentMode", "Value": getString(filteredRecords, "PaymentMode") });
            parameters.push({ "Key": "BankId", "Value": getString(filteredRecords, "BankId") });
            parameters.push({ "Key": "PlantId", "Value": getString(filteredRecords, "PlantId") });
            parameters.push({ "Key": "EntityId", "Value": getString(filteredRecords, "EntityId") });
            parameters.push({ "Key": "DepartmentId", "Value": getString(filteredRecords, "DepartmentId") });
            parameters.push({ "Key": "DesignationId", "Value": getString(filteredRecords, "DesignationId") });
            parameters.push({ "Key": "SectionId", "Value": getString(filteredRecords, "SectionId") });
            parameters.push({ "Key": "SubSectionId", "Value": getString(filteredRecords, "SubSectionId") });

            var PaymentModeList = parameters[0].Value;
            var BankList = parameters[1].Value;
            var PlantList = parameters[2].Value;
            var EntityList = parameters[3].Value;
            var DepartmentList = parameters[4].Value;
            var DesignationList = parameters[5].Value;
            var SectionList = parameters[6].Value;
            var SubSectionList = parameters[7].Value;

            var DropDownListMonth = $("#ddlMonthList").data("ejDropDownList");
            var DropDownListYear = $("#ddlYearList").data("ejDropDownList");
            $scope.month = DropDownListMonth.getSelectedValue();
            $scope.year = DropDownListYear.getSelectedValue();

            $http({
                method: 'POST',
                url: $scope.path + 'CwReport',
                data: {
                    'PaymentModeList': PaymentModeList, 'BankList': BankList
                    , 'PlantList': PlantList, 'EntityList': EntityList
                    , 'DepartmentList': DepartmentList, 'DesignationList': DesignationList
                    , 'SectionList': SectionList, 'SubSectionList': SubSectionList
                    , 'Month': $scope.month, 'Year': $scope.year
                    ,'isActive': $scope.isActive,
                    'isSeperated': $scope.isSeperated,
                    'isMaternity': $scope.isMaternity,
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);//downloadgriddataUrlPath
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
}