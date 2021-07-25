'use strict';
ExternalDataUploadFromExcelController.$inject = ['$scope', '$http', '$location', "$rootScope", '$window', "$compile", 'baseService', 'fileReader'];
function ExternalDataUploadFromExcelController($scope, $http, $location, $rootScope, $window, $compile, baseService, fileReader) {
    $scope.path = 'Payrolls/ExternalDataUploadFromExcel/';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $rootScope.title = 'Salary Structure Data Upload';
    $scope.SaveDataList = []


    $scope.LoadData = function () {
        try {




            //if (baseService.isUndefinedOrNull($scope.ModelNew.SalaryHeadId)) {
            //    throw "Please Select Salary Head.";
            //}

            if (baseService.isUndefinedOrNull($scope.ModelNew.YearNo)) {
                throw "Please Select Year.";
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.MonthNo) || $scope.ModelNew.MonthNo == 0) {
                throw "Please Select Month.";
            }




            $http({
                method: 'GET',
                url: $scope.path + 'LoadData?SalaryHeadId=' + $scope.ModelNew.SalaryHeadId + '&MonthNo=' + $scope.ModelNew.MonthNo + '&YearNo=' + $scope.ModelNew.YearNo

            }).then(function successCallback(response) {
                if (response.data.Error === true) {

                    ShowResult(response.data.Message, "failure");

                }
                else {
                    $scope.SaveDataList = [];
                    $scope.SaveDataList = response.data;

                }
            }, function errorCallback(response) {

            });
            return true;


        } catch (e) {

            ShowResult(e, "failure");
        }
    };


    $scope.tabh = 11;
    $scope.setTab11 = function (newTab) {
        $scope.tabh = newTab;
        $scope.employees = [];

    };
    $scope.isSet11 = function (tabNum) {
        return $scope.tabh === tabNum;
    };
    $scope.setTab22 = function (newTab) {
        $scope.tabh = newTab;

    };
    $scope.isSet22 = function (tabNum) {
        return $scope.tabh === tabNum;
    };


    $scope.AttdnRawData = [];
    $scope.picdata = null;
    $scope.ShowSaveBtn = false;
    $("#uploadImage").change(function () {
        $scope.picdata = this.files[0];
    });

    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };

    $scope.ModelNew = {
        Id: null,
        FileName: null,
        SalaryHeadId: null,
        YearNo: null,
        MonthNo: null

    };
    //$scope.CustomModel = {
    //       SalaryHeadId: null,
    //       YearNo: null,
    //       MonthNo: null
    //   };

    $scope.ShowSaveBtn = false;

    $scope.ModelNew.YearNo = new Date().getFullYear().toString();
    $scope.ModelNew.MonthNo = new Date().getMonth().toString();

    $scope.ImportData = function () {
        try {


            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                var picData = new FormData();
                if (!baseService.isUndefinedOrNull($scope.picdata)) {
                    $scope.ModelNew.FileName = $scope.picdata.name;
                }

                if (baseService.isUndefinedOrNull($scope.ModelNew.SalaryHeadId)) {
                    throw "Please Select Salary Head.";
                }

                if (baseService.isUndefinedOrNull($scope.ModelNew.YearNo)) {
                    throw "Please Select Year.";
                }

                if (baseService.isUndefinedOrNull($scope.ModelNew.MonthNo)) {
                    throw "Please Select Month.";
                }




                $http({
                    method: 'POST',
                    url: $scope.path + 'ImportData',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        picData.append("modelNew", angular.toJson(data.modelNew));
                        if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                            picData.append('file', data.file);
                        }
                        return picData;
                    },
                    data: {
                        'modelNew': $scope.ModelNew
                        , 'file': $scope.picdata
                        //, 'pSalaryHeadId': $scope.ModelNew.SalaryHeadId 
                        //, 'pYearNo': $scope.ModelNew.YearNo 
                        //, 'pMonthNo': $scope.ModelNew.MonthNo 
                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.ShowSaveBtn = false;
                        ShowResult(response.data.Message, "failure");

                    }
                    else {
                        $scope.AttdnRawData = [];
                        $scope.AttdnRawData = response.data;
                        $scope.ShowSaveBtn = true;
                    }
                }, function errorCallback(response) {

                });
                return true;

            }
        } catch (e) {

            ShowResult(e, "failure");
        }
    };


    $scope.onrowdatabound = function (e) {
        if (e.data.Remarks !== '')
            e.row.css("background-color", "red");
    };



    $scope.save = function () {

        try {
            for (var i = 0; i < $scope.AttdnRawData.length; i++) {

                if ($scope.AttdnRawData[i].Remarks !== '') {
                    throw "Please Upload valied data";
                }

            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.SalaryHeadId)) {
                throw "Please Select Salary Head.";
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.YearNo)) {
                throw "Please Select Year.";
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.MonthNo)) {
                throw "Please Select Month.";
            }

            $.ajax({
                type: "POST",
                url: $scope.path + 'SaveExternalData',
                data: {
                    'data': $scope.AttdnRawData
                    , 'SalaryHeadId': $scope.ModelNew.SalaryHeadId
                    , 'YearNo': $scope.ModelNew.YearNo
                    , 'MonthNo': $scope.ModelNew.MonthNo
                },
                dataType: "json",
                success: function (response) {


                    if (response.Error === true) {
                        $scope.ShowSaveBtn = false;
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        ShowResult(response.Message, 'success');
                        $scope.AttdnRawData = [];
                        $("#uploadImage").val(null);
                        $scope.ShowSaveBtn = false;
                    }

                }

            });

        } catch (e) {
            $scope.ShowSaveBtn = false;
            ShowResult(e, 'failure');

        }
    };



    //$scope.GetSampleFile = function () {



    //    $http({
    //        method: 'GET',
    //        url: "Attendances/AttendanceRawDataUpload/GetSampleFile",
    //        headers: {
    //            'Content-Type': 'application/json'
    //        }
    //    }).then(function successCallback(response) {
    //        if (response.data.Error === true) {
    //            ShowResult(response.data.Message, 'failure');
    //        }

    //    }), function errorCallBack(response) {
    //        ShowResult(response.data.Message, 'failure');
    //    };
    //};
    $scope.GetSampleFile = function () {
        var ReportFormat = 'Excel';
        location.href = $scope.path + 'GetSampleFile?reportFormat=' + ReportFormat;
    };



    $scope.SalaryHeadList = [];
    $scope.getSalaryHeadListList = function () {
        $http.get($scope.path + 'GetSalaryHeadListeList')
            .then(function (response) {
                $scope.SalaryHeadList = response.data;
            });
    };
    $scope.getSalaryHeadListList();
    $scope.monthList = [
        {
            Value: '1',
            Text: 'January'
        },
        {
            Value: '2',
            Text: 'February'
        },
        {
            Value: '3',
            Text: 'March'
        },
        {
            Value: '4',
            Text: 'April'
        },
        {
            Value: '5',
            Text: 'May'
        },
        {
            Value: '6',
            Text: 'June'
        },
        {
            Value: '7',
            Text: 'July'
        },
        {
            Value: '8',
            Text: 'August'
        },
        {
            Value: '9',
            Text: 'September'
        },
        {
            Value: '10',
            Text: 'October'
        },
        {
            Value: '11',
            Text: 'November'
        },
        {
            Value: '12',
            Text: 'December'
        }
    ];



    $scope.yearList = [];
    //cboService.getCboLeaveYear(function (result) {
    //    $scope.yearList = result;
    //});



    function GetYearList() {
        var FromYear = 2017
        var ToYear = parseInt(new Date().getFullYear().toString());
        while (FromYear <= ToYear) {
            $scope.yearList.push(FromYear);
            FromYear++;
        }
    }
    GetYearList();

    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath
    $scope.DownloadReport = function () {
        try {
            $scope.fileName = "ExternalDataUploadFromExcel.xls";
            if ($scope.SaveDataList.length == 0) {
                throw "Load Data first..";
            }
            var parameters = [];
            var gridObj = $("#GridUploadedData").data("ejGrid");
            var filteredRecords = gridObj.getFilteredRecords();
            if (filteredRecords.length == 0) {
                filteredRecords = $scope.SaveDataList;
            }
            
            parameters.push({ "Key": "SystemId", "Value": getString(filteredRecords, "SystemId") });
            parameters.push({ "Key": "SalaryHeadID", "Value": getString(filteredRecords, "SalaryHeadID") });
            parameters.push({ "Key": "HeadType", "Value": getString(filteredRecords, "HeadType") });
            parameters.push({ "Key": "EntryCurrencyID", "Value": getString(filteredRecords, "EntryCurrencyID") });
            parameters.push({ "Key": "EntryAmount", "Value": getString(filteredRecords, "EntryAmount") });

            $http({
                method: 'POST',
                url: 'Payrolls/ExternalDataUploadFromExcel/ExternalDataUploadReport',
                data: {
                    'EmployeeList': parameters[0].Value, 'SalaryHeadId': $scope.ModelNew.SalaryHeadId, 'MonthNo': $scope.ModelNew.MonthNo, 'YearNo': $scope.ModelNew.YearNo
                    , 'SalaryHeadIDs': parameters[1].Value, 'HeadType': parameters[2].Value, 'CurrencyID': parameters[3].Value, 'EntryAmount': parameters[4].Value,
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
    };
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
}





