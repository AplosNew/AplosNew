'use strict';
ProductIntegrityAnalysisController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function ProductIntegrityAnalysisController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "ProductIntegrityAnalysis";
    $scope.Action = 'Save';
    $scope.AnalysistypeList = [];
    $scope.AnalysistypefilterList = [];
    $scope.path = 'OrderManagements/ProductIntegrityAnalysis/';
    $scope.saveUrl = $scope.path + 'Create';
    $scope.savePresentyNamesUrl = $scope.path + 'CreatePresentyNames';
    $scope.saveAnalysisItemUrl = $scope.path + 'CreateAnalysisItem';
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
   
    $scope.status = {
        Id: null,
        FromDate: null,
        ToDate: $filter('dateFiltering')(date, 'dd-MM-yyyy'),
        Customer: null,
        AnalysisTypeFilter: null
    };
    $scope.statusNew = Object.assign({}, $scope.status);

    $scope.Analysis = {
        Id: null,
        LineItemId: null,
        AnalysisMasterId: null,
        AnalysisType: null,
        Remarks: null
    };
    $scope.AnalysisNew = Object.assign({}, $scope.Analysis);

    $scope. AnalysistypeList= [
        {
            'Value': 'OrderAnalysis',
            'Text': 'OrderAnalysis'
        },
        {
            'Value': 'PreProduction',
            'Text': 'PreProduction'
        }
        ,
        {
            'Value': 'PostProduction',
            'Text': 'PostProduction'
        }
        ,
        {
            'Value': 'Others',
            'Text': 'Others'
        }
    ];
    $scope.AnalysistypefilterList = [
        {
            'Value': 'OrderAnalysis',
            'Text': 'OrderAnalysis'
        },
        {
            'Value': 'PreProduction',
            'Text': 'PreProduction'
        }
        ,
        {
            'Value': 'PostProduction',
            'Text': 'PostProduction'
        }
        ,
        {
            'Value': 'Others',
            'Text': 'Others'
        }
    ];
    $scope.GetFromDateList = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductIntegrityAnalysis/GetFromDateList'
        }).then(function successCallback(response) {
            $scope.statusNew.FromDate = response.data[0].FromDate;
        });
    }
    $scope.GetFromDateList();

    $scope.CustomerList=[];
    $scope.GetCustomerList = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductIntegrityAnalysis/GetCustomerList'
        }).then(function successCallback(response) {
            $scope.CustomerList = response.data;
        });
    }
    $scope.GetCustomerList();

    $scope.AnalysisNameList = [];
    $scope.GetAnalysisNameList = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductIntegrityAnalysis/GetAnalysisNameList'
        }).then(function successCallback(response) {
            $scope.AnalysisNameList = response.data;
        });
    }
    $scope.GetAnalysisNameList();
    
    $scope.ProductIntegrityAnalysisList = [];
    $scope.View = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.statusNew.ToDate)) {
                throw "To Date is required.";
            }

           $scope.ProductIntegrityAnalysisList = [];
            $http({
                method: 'POST',
                url: $scope.path + "LoadProductIntegrityAnalysis",
                data: { 'CustomerInfo': $scope.statusNew.Customer, 'todate': $scope.statusNew.ToDate, 'fromDate': $scope.statusNew.FromDate, 'AnalysisType': $scope.statusNew.AnalysisTypeFilter},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.ProductIntegrityAnalysisList = response.data;
                    var gridObj = $("#GridProductIntegrityAnalysis").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.OrderAnalysisList = [];
    $scope.OrderAnalysisView = function (AnalysisType) {
        try {
            $scope.OrderAnalysisList = [];
            $http({
                method: 'POST',
                url: $scope.path + "LoadOrderAnalysis",
                data: { 'AnalysisType': AnalysisType},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.OrderAnalysisList = response.data;
                    var gridObj = $("#GridOrderAnalysis").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.ProductIntegrityAnalysisDetailsList = [];
    $scope.GetDetails = function (args) {
        $scope.Clear();
        $scope.NewObject = args.data;
        $http({
            method: 'POST',
            url: $scope.path + "LoadProductIntegrityAnalysisByLineItem",
            data: { 'LineItemId': $scope.NewObject.LineItemId, 'AnalysisType': $scope.statusNew.AnalysisTypeFilter },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ProductIntegrityAnalysisDetailsList = response.data;
            var gridObj = $("#GridProductIntegrityAnalysisDetails").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#ActualDetailsPopUp')).modal('show');
        }
        )
    }


    $scope.GetOrderAnalysisDetails = function (args) {
        $scope.Clear();
        $scope.NewObject = args.data;
        $http({
            method: 'POST',
            url: $scope.path + "LoadOrderAnalysisByLineItem",
            data: { 'LineItemId': $scope.NewObject.LineItemId, 'AnalysisType': $scope.NewObject.AnalysisType  },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ProductIntegrityAnalysisDetailsList = response.data;
            $scope.AnalysisNew.AnalysisMasterId = $scope.ProductIntegrityAnalysisDetailsList[0].AnalysisMasterId;
            $scope.AnalysisNew.AnalysisType = $scope.ProductIntegrityAnalysisDetailsList[0].AnalysisType;
            $scope.AnalysisNew.Remarks = $scope.ProductIntegrityAnalysisDetailsList[0].Remarks;
            $scope.AnalysisNew.Id = $scope.ProductIntegrityAnalysisDetailsList[0].Id;
            $scope.LoadItemDetails();
            var gridObj = $("#GridProductIntegrityAnalysisDetails").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#ActualDetailsPopUp')).modal('show');
        }
        )
    }

    $scope.closeActualDetailsPopUp = function () {
        angular.element(document.querySelector('#ActualDetailsPopUp')).modal('hide');
    }

    $scope.Clear = function () {
        PIAClearFields();
    };

    function PIAClearFields() {
        $scope.AnalysisNew = Object.assign({}, $scope.Analysis);
        $scope.ProductAnalysisItemList = [];
    }
   
    $scope.ProductAnalysisItemList = [];
    $scope.ItemId = null;
    $scope.LoadItemDetails = function () {
        $http({

            method: 'Get',
            url: 'OrderManagements/ProductIntegrityAnalysis/LoadItemDetails?ProductId=' + $scope.AnalysisNew.AnalysisMasterId + '&Pid=' + $scope.AnalysisNew.Id
        }).then(function successCallback(response) {
            $scope.ProductAnalysisItemList = response.data;
        }
        )
    }

    $scope.selectResponsiblePerson = function (data) {
        $scope.Newobject = data.data;
        $scope.getEmployee();
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('show');
    }

    $scope.EmployeeList = [];
    $scope.getEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EmployeeList = resp.data;
        });
    }

    $scope.doubleEmployee = function (e) {
        $scope.Newobject.ResponsiblePersonId = e.data.SystemId;
        $scope.Newobject.ResponsiblePerson = e.data.EmployeeName;
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }

    $scope.closeResponsiblePersonPopUp = function () {
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }

    $scope.selectItemValue = function (data) {
        $scope.Newobject = data.data;
        $scope.getItemValue();
        angular.element(document.querySelector('#ItemValuePopup')).modal('show');
    }

    $scope.ItemValueList = [];
    $scope.getItemValue = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetAnalysisItemValueList',
            data: { 'ItemId': $scope.Newobject.ItemId},
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ItemValueList = resp.data;
        });
    }

    $scope.doubleItemValue = function (e) {
        $scope.Newobject.ValueId = e.data.Value;
        $scope.Newobject.Value = e.data.Text;
        angular.element(document.querySelector('#ItemValuePopup')).modal('hide');
    }

    $scope.closeItemValuePopUp = function () {
        angular.element(document.querySelector('#ItemValuePopup')).modal('hide');
    }

    $scope.refreshTemplatePresentyNames = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllPresentyNames });
    };
    function CheckBoxSelectAllPresentyNames(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridPresentyNamesPopUp").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.PresentyNamesList.length; i++) {
                $scope.PresentyNamesList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridPresentyNamesPopUp").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.SearchPresentyName = function () {
        $scope.getPresentyNames();
        angular.element(document.querySelector('#PresentyNamesPopup')).modal('show');
    }

    $scope.PresentyNamesList = [];
    $scope.getPresentyNames = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetPresentyNames',
            data: { 'PId': $scope.AnalysisNew.Id },
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.PresentyNamesList = resp.data;
        });
    }

    $scope.ClosePresentyNamesPopUp = function () {
        angular.element(document.querySelector('#PresentyNamesPopup')).modal('hide');
    }

    $scope.SavePresentyNames = function () {
        try {

            $scope.SavePresentyNamesList = [];
            for (var i = 0; i < $scope.PresentyNamesList.length; i++) {
                if ($scope.PresentyNamesList[i].Flag == true) {
                    $scope.SavePresentyNamesList.push($scope.PresentyNamesList[i]);
                }
            }

            $http({
                method: 'POST',
                url: $scope.savePresentyNamesUrl,
                data: {
                    "DataList": $scope.SavePresentyNamesList,
                    "PId": $scope.AnalysisNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };
    
    $scope.SaveAnalysisItem = function () {
        try {

            $scope.SaveAnalysisItemList = [];
            for (var i = 0; i < $scope.ProductAnalysisItemList.length; i++) {
                if ($scope.ProductAnalysisItemList[i].Applicable == true) {
                    $scope.SaveAnalysisItemList.push($scope.ProductAnalysisItemList[i]);
                }
            }

            $http({
                method: 'POST',
                url: $scope.saveAnalysisItemUrl,
                data: {
                    "DataList": $scope.SaveAnalysisItemList,
                    "PId": $scope.AnalysisNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadItemDetails();
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

   
    $scope.SaveAnalysisHeader = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'AnalysisHeaderData': $scope.AnalysisNew, 'LineItemId': $scope.NewObject.LineItemId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.AnalysisNew.Id = response.data.Data.Id;
                    $scope.AnalysisNew.AnalysisMasterId = response.data.Data.AnalysisMasterId;
                    $scope.LoadItemDetails();
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
                
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    //#region MOI File 
    $scope.ItemId = null;
    $scope.onBeginUpload = function (args) {
        try {
            if (angular.isUndefinedOrNull(args.model.Data))
                throw 'Please select/save the order first'
            $scope.ItemId = args.model.Data;
            args.data = args.model.Data;
        } catch (e) {

            args.cancel = true;
            ShowResult(e, 'Error');
        }

    }
    $scope.uploadUrl = "OrderManagements/ProductIntegrityAnalysis/SaveDefault";
    $scope.fileselect = function (e) {

    }
    $scope.errorPicUpload = function (e) {
        if (angular.isUndefinedOrNull($scope.ItemId))
            ShowResult('Please select/save the order first', 'Error');
        else
            ShowResult("The selected file size is too large. Please select a file less than " + Math.round(e.model.fileSize / (1024 * 1024)) + "MB", 'failure');
    }

    $scope.FileDownload = function (data,test) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
         $scope.dwonloadUrl = virtualPath.PAIPath + '/' + data.Id + extention;
    };

    $scope.getFileList = function () {
        $http({
            method: 'Get',
            url: 'OrderManagements/ProductIntegrityAnalysis/LoadItemDetails?ProductId=' + $scope.AnalysisNew.AnalysisMasterId + '&Pid=' + $scope.AnalysisNew.Id
        }).then(function successCallback(response) {
            $scope.ProductAnalysisItemList = response.data;
            var gridObj = $("#GridItem").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#ActualDetailsPopUp')).modal('show');
        }
        )
    }
    //#endregion
}

