'use strict';
LWQRUpdateController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$window"];
function LWQRUpdateController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "LWQRUpdate";
    $scope.Action = 'Save';
    $scope.path = 'QMS/LWQRUpdate/';

    $scope.status = {
        Id: null,
        Customer: null,
        CustomerId: null,
        InvoiceNo: null,
        InvoiceId: null,
        ProductionOrderId: null,
        LotNo: null,
        ByWhomId: null,
        ByWhom: null,
        UserName: null,
        Remarks: null,
        SpecialRemarks: null,
    };
    $scope.statusNew = Object.assign({}, $scope.status);

    $scope.comment = {
        QRGrade: null,
        QRComment: null,
        QRByWhom: null,
        OWGrade: null,
        OWByWhom: null,
        OWCommentDetails: null,
    };
    $scope.commentNew = Object.assign({}, $scope.comment);


    $scope.CustomerList = [];
    $scope.selectCustomer = function () {
        $http({
            method: 'GET',
            url: 'QMS/LWQRUpdate/GetUpdateCustomerList'
        }).then(function successCallback(response) {
            $scope.CustomerList = response.data;
            angular.element(document.querySelector('#CustomerPopup')).modal('show');
        });
    }

    $scope.doubleCustomer = function (e) {
        $scope.statusNew.CustomerId = e.data.PartyId;
        $scope.statusNew.Customer = e.data.Customer;
        angular.element(document.querySelector('#CustomerPopup')).modal('hide');
    }

    $scope.closeCustomerPopup = function () {
        angular.element(document.querySelector('#CustomerPopup')).modal('hide');
    }

    $scope.InvoiceList = [];
    $scope.selectInvoice = function () {
        $http({
            method: 'GET',
            url: 'QMS/LWQRUpdate/GetUpdateInvoiceList?PartyId=' + $scope.statusNew.CustomerId
        }).then(function successCallback(response) {
            $scope.InvoiceList = response.data;
            angular.element(document.querySelector('#InvoicePopup')).modal('show');
        });
    }

    $scope.doubleInvoice = function (e) {
        $scope.statusNew.InvoiceId = e.data.InvoiceId;
        $scope.selectPONo();
        angular.element(document.querySelector('#InvoicePopup')).modal('hide');
    }

    $scope.closeInvoicePopup = function () {
        angular.element(document.querySelector('#InvoicePopup')).modal('hide');
    }

    $scope.POList = [];
    $scope.selectPONo = function () {
        $scope.POList = [];
        $http({
            method: 'GET',
            url: 'QMS/LWQRUpdate/GetUpdatePOList?InvoiceId=' + $scope.statusNew.InvoiceId
        }).then(function successCallback(response) {
            $scope.POList = response.data;
        });
    }
    $scope.selectPONo();

    //$scope.LotNumberLists = [];
    //$scope.GetLotNumberLists = function () {
    //    $scope.LotNumberLists = [];
    //    $http({
    //        method: 'GET',
    //        url: 'QMS/LWQRUpdate/GetUpdateLotNumberLists?POId=' + $scope.statusNew.ProductionOrderId
    //    }).then(function successCallback(response) {
    //        $scope.LotNumberLists = response.data;
    //    });
    //}
    //$scope.GetLotNumberLists();

    $scope.LWQRList = [];
    $scope.View = function () {
        try {
            if ($scope.statusNew.LotNo === null) {
                throw "LotNumber is must please select it and proceed..";
            }
            else {
                $http.get('QMS/LWQRUpdate/LoadLWQRUpdate?POId=' + $scope.statusNew.ProductionOrderId + '&LotNumber=' + $scope.statusNew.LotNo + '&CustomerId=' + $scope.statusNew.CustomerId + '&InvoiceId=' + $scope.statusNew.InvoiceId)
                    .then(function (response) {
                        $scope.LWQRList = response.data;
                        $scope.commentNew.QRComment = $scope.LWQRList[0].QRComment;
                        $scope.commentNew.QRByWhom = $scope.LWQRList[0].QRByWhom;
                        $scope.commentNew.QRGrade = $scope.LWQRList[0].QRGrade;
                        $scope.commentNew.OWComment = $scope.LWQRList[0].OWComment;
                        $scope.commentNew.OWByWhom = $scope.LWQRList[0].OWByWhom;
                        $scope.commentNew.OWGrade = $scope.LWQRList[0].OWGrade;
                        $scope.statusNew.Customer = $scope.LWQRList[0].CustomerName;
                        $scope.statusNew.CustomerId = $scope.LWQRList[0].CustomerId;
                        $scope.statusNew.ProductionOrderId = $scope.LWQRList[0].ProductionOrderId;
                        $scope.statusNew.LotNo = $scope.LWQRList[0].LotNo;
                        $scope.statusNew.InvoiceNo = $scope.LWQRList[0].InvoiceId;
                        $scope.statusNew.UserName = $scope.LWQRList[0].UserName;
                        $scope.statusNew.ByWhomId = $scope.LWQRList[0].ByWhomId;
                        $scope.statusNew.ByWhom = $scope.LWQRList[0].ByWhom;
                        $scope.statusNew.Remarks = $scope.LWQRList[0].Remarks;
                        $scope.statusNew.Id = $scope.LWQRList[0].CQRHeaderId;
                    });
                angular.element(document.querySelector('#CustomerQualityReportPopup')).modal('show');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.closeCustomerQualityReportPopup = function () {
        angular.element(document.querySelector('#CustomerQualityReportPopup')).modal('hide');
        LWQRClearFields();
    }

    $scope.ByWhomList = [];
    $scope.selectByWhom = function () {
        $http({
            method: 'GET',
            url: 'QMS/LWQRUpdate/GetByWhomList'
        }).then(function successCallback(response) {
            $scope.ByWhomList = response.data;
            angular.element(document.querySelector('#ByWhomPopup')).modal('show');
        });
    }

    $scope.doubleByWhom = function (e) {
        $scope.statusNew.ByWhomId = e.data.SystemId;
        $scope.statusNew.ByWhom = e.data.EmployeeName;
        angular.element(document.querySelector('#ByWhomPopup')).modal('hide');
    }

    $scope.closeByWhomPopup = function () {
        angular.element(document.querySelector('#ByWhomPopup')).modal('hide');
    }

    $scope.Clear = function () {
        LWQRClearFields();
    };

    function LWQRClearFields() {
        $scope.Action = "Save";
        $scope.statusNew = Object.assign({}, $scope.status);
        $scope.commentNew = Object.assign({}, $scope.comment);
        $scope.LWQRList = [];
        $scope.selectPONo();
        //$scope.GetLotNumberLists();
    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Parameter === id) {
                return true;
            }
        }
        return false;
    }

    $scope.SaveCQR = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.LWQRList.length; i++) {
                if ($scope.LWQRList[i].Finalreport === true) {
                    if (checkExists($scope.SaveList, $scope.LWQRList[i].Parameter) == false) {
                        $scope.SaveList.push($scope.LWQRList[i]);
                    }

                }
            }
            $http({
                method: "POST",
                url: 'QMS/LWQRUpdate/CreateCQRData',
                data: {
                    'data': $scope.statusNew,
                    'DataList': $scope.SaveList
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.View();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.UpdateSpecialRemarks = function () {
        try {
           
            $http({
                method: "POST",
                url: 'QMS/LWQRUpdate/UpdateParameterData',
                data: {
                    'ParameterChildId': $scope.ParameterChildId,
                    'SpecialRemarks': $scope.statusNew.SpecialRemarks
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "updated");
                    $scope.View();
                    $scope.statusNew.SpecialRemarks = null;
                    angular.element(document.querySelector('#SpecialRemarksPopup')).modal('hide');
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.ParameterChildId = null;
    $scope.SpecialRemarks = function (data) {
        try {
            if (data.data.Id === null) {
                throw "Please save the record and proceed..";
            }
            else
            { 
            if (data.data.SpecialRemarks === null) {
                $scope.ParameterChildId = data.data.Id;
                angular.element(document.querySelector('#SpecialRemarksPopup')).modal('show');
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SpecialRemarksPopup = function () {
        angular.element(document.querySelector('#SpecialRemarksPopup')).modal('hide');
    }

    $scope.JobCardCQReportFunc = function () {
        try {
            //var gridObj = $("#GridCustomerQualityReport").ejGrid("instance");
            //var filtereddata = gridObj.getFilteredRecords();
            //if (filtereddata.length == 0) {
            //    filtereddata = $scope.LWQRList;
            //}
            //$scope.LWQRListNew = [];
            //for (var i = 0; i < filtereddata.length; i++) {
            //    $scope.LWQRListNew.push(filtereddata[i]);
            //}

            //if ($scope.LWQRListNew.length > 500) {
            //    throw "Maximum 50 'Job card' can be downloded at a time";
            //}
            //else {
                var url = $scope.path + '/GetCustomerQualityLotWiseUpdateJobCardReport?CustomerId=' + $scope.statusNew.CustomerId + '&InvoiceId=' + $scope.statusNew.InvoiceId + '&ProductionOrderId=' + $scope.statusNew.ProductionOrderId + '&LotNumber=' + $scope.statusNew.LotNo;
                $rootScope.report(url);
           /* }*/
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.LotNumberLists = [];
    $scope.selectLotNo = function () {
        $scope.LotNumberLists = [];
        $http({
            method: 'GET',
            url: 'QMS/LWQRUpdate/GetUpdateLotNumberLists?POId=' + $scope.statusNew.ProductionOrderId
        }).then(function successCallback(response) {
            $scope.LotNumberLists = response.data;
            angular.element(document.querySelector('#LotNumberPopup')).modal('show');
        });
    }

    $scope.doubleLotNumber = function (e) {
        $scope.statusNew.LotNo = e.data.LotNumber;
        $scope.statusNew.ProductionOrderId = e.data.PONo;
        angular.element(document.querySelector('#LotNumberPopup')).modal('hide');
    }

    $scope.closeLotNumberPopup = function () {
        angular.element(document.querySelector('#LotNumberPopup')).modal('hide');
    }
}

