'use strict';
//grnApprovalController.$inject = ['addressService', 'commonMessage', 'cboService', '$scope', '$rootScope', 'baseService', '$routeParams', '$http'];
//function grnApprovalController(addressService, commonMessage, $scope, $rootScope, cboService, baseService, $routeParams, $http) {
grnApprovalController.$inject = ['accountService', 'addressService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller', '$location'];
function grnApprovalController(accountService, addressService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, $location) {
    $rootScope.title = "Inventory Approved";
    $rootScope.title = "GRN Check / Uncheck";
    $scope.modelList = [];
    //$scope.path = 'Products/InventoryReceive/';
    $scope.path = 'Products/GoodsReceiveNote/';


    //#region GRNApplrval Detail
    $scope.lst = [];
    $scope.POListDetails = function () {
        //debugger;
        $http({
            method: 'GET',
            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
            url: 'Products/GoodsReceiveNote/GRNDetailsData'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            //$scope.detailgrid($scope.lst);
            window.lst = response.data;

        });
    }
    $scope.POListDetails();
    $scope.GRNDocumentMapDataAll = function () {
        //debugger;
        $http({
            method: 'GET',
            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
            url: 'Products/GoodsReceiveNote/GRNDocumentMapDataAll'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            //$scope.detailgrid($scope.lst);
            window.Img = response.data;

        });
    }
    $scope.GRNDocumentMapDataAll();

    $scope.data1 = $scope.lst;
    $scope.detailTemp = "#tabGridContents";
    //$scope.detailgrid = "detailGridData(e)";
    $scope.detailgrid = function detailGridData(e) {
        //debugger;

        var filteredData = e.data["Id"];
        var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("InventoryReceiveId", "equal", parseInt(filteredData), true).take(200));
        e.detailsElement.find("#detailGrid").ejGrid({

            dataSource: data,
			columns: ["MaterialGroupName", "MaterialName", "Article", "SKU1", "SKU2", "SKU3", "MaterialDetail", "TransactionQty", "TransactionUoM", "TransactionRate", "CurrencyName", "TotalMaterialTranAmount"]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
        //var filteredData1 = e.data["Id"];
        var dataImg = ej.DataManager(window.Img).executeLocal(ej.Query().where("GRNId", "equal", parseInt(filteredData), true).take(100));
        e.detailsElement.find("#detailGrid1").ejGrid({
            dataSource: dataImg,
            columns: [{ field: "UserFilename", headerText: "UserFilename", width: 100 },
            { field: "Description", headerText: "Description", width: 100 },
            { field: "Remarks", headerText: "Remarks", width: 100 },

            ]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }
    //#endregion


    $scope.LoadapprovalStatus = function () {
        cboService.getEnumCbo("enum/GetCheckedStatusCbo", function (result) {
            $scope.approvalStatusList = result;
        });

    }
    $scope.LoadapprovalStatus();


    $scope.updateUrl = $scope.path + 'approved';
//Taufik Grn APP and UNAP for Update
    $scope.updateUrl1 = $scope.path + 'Approved1';


    $scope.searchGrnList = [
        {
            value: 'PartyCode'
            , name: 'Vendor Code'
        },
        {
            value: 'PartyName'
            , name: 'Vendor Name'
        },
        {
            value: 'PartyAccountGroupName'
            , name: 'Account Group'
        },
        {
            value: 'Id'
            , name: 'GRN No'
        },
        {
            value: 'GRNDate'
            , name: 'GRN Date'
        },
        {
            value: 'DocRefNo'
            , name: 'Vendor DocRefNo'
        },
        {
            value: 'InvoiceNo'
            , name: 'Invoice No'
        },
        {
            value: 'InvoiceDate'
            , name: 'Invoice Date'
        }
    ];

    
  
    $scope.grnPopUp = function () {
        $scope.popUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'PartyCode'
            , searchBy: "PartyCode"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.grnList = [];
        $rootScope.tempList = [];
        angular.forEach($scope.modelList, function (a) {
            $rootScope.tempList.push({
                Id: a.Id
                , PartyCode: a.PartyCode
                , PartyName: a.PartyName
                , PartyAccountGroupName: a.PartyAccountGroupName
                , GRNDate: a.GRNDate
                , DocRefNo: a.DocRefNo
                , InvoiceNo: a.InvoiceNo
                , InvoiceDate: a.InvoiceDate
                , TransactionQty: a.TransactionQty
                , TransactionAmount: a.TransactionAmount
                , BaseAmount: a.BaseAmount            
                , IsApproved: a.IsApproved
                , FlagStatus: $scope.GRN
            });
        });
        baseService.setCurrentPage('grnList');
        $scope.getGrnData = function (pageno) {
            baseService.paginationBase($scope.path + 'GetList', pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.grnList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    for (var t = 0; t < baseService.arrayLength($scope.grnList); t++) {
                        $scope.grnList[t].Flag = baseService.valueCheckInList($rootScope.tempList, 'Id', $scope.grnList[t].Id);
                    }
                    angular.element(document.querySelector('#grnPopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };

       // $scope.getGrnData();
        $scope.getGrnDataById();
    };
    $scope.grnList = [];
    //$scope.GRN = 0;
    $scope.getGrnDataById = function (pageno) {
        baseService.paginationBase($scope.path + 'GetListByGrnno?GRN=' + $scope.GRN, pageno, $scope.popUpParameters)
            .then(function (result) {
                $scope.grnList = [];
                $scope.grnList = result.Rows;
                $scope.popUpParameters.total_count = result.Total;
                for (var t = 0; t < baseService.arrayLength($scope.grnList); t++) {
                    $scope.grnList[t].Flag = baseService.valueCheckInList($rootScope.tempList, 'Id', $scope.grnList[t].Id);
                }
                angular.element(document.querySelector('#grnPopUp')).modal('show');
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.grnAdd = function () {
       
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            angular.forEach($rootScope.tempList, function (a) {
                if (!baseService.valueCheckInList($scope.modelList, 'Id', a.Id)) {
                    $scope.modelList.push({
                        Id: null
                        , Id: a.Id
                        , PartyCode: a.PartyCode
                        , PartyName: a.PartyName
                        , PartyAccountGroupName: a.PartyAccountGroupName
                        , GRNDate: a.GRNDate
                        , DocRefNo: a.DocRefNo
                        , InvoiceNo: a.InvoiceNo
                        , InvoiceDate: a.InvoiceDate
                        , TransactionQty: a.TransactionQty
                        , TransactionAmount: a.TransactionAmount
                        , BaseAmount: a.BaseAmount
                        , IsApproved: a.IsApproved
                        , FlagStatus: $scope.GRN
                    });
                }
            });
        }
        else
            $scope.modelList = [];
        angular.forEach($scope.modelList, function (a) {
            if (!baseService.valueCheckInList($rootScope.tempList, 'Id', a.Id))
                $scope.modelList.splice(a, 1);
        });
        $scope.closeGrnPopUp();
    };

    $scope.closeGrnPopUp = function () {
        angular.element(document.querySelector('#grnPopUp')).modal('hide');
    }

    $scope.removeRowModal = function (name, index, listName, tempId, listId) {
        try {
            $scope.popUpIndex = index;
            $scope.listName = listName;
            $scope.tempId = tempId;
            $scope.listId = listId;
            $scope.message_confirmation = "Are you sure want to remove [" + name + "] ";
            angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    }
    $scope.removeRow = function () {
        for (var t = 0; t < baseService.arrayLength($rootScope.tempList); t++) {
            if ($rootScope.tempList[t][$scope.tempId] === $scope[$scope.listName][$scope.popUpIndex][$scope.listId])
                $rootScope.tempList.splice(t, 1);
        }
        $scope[$scope.listName].splice($scope.popUpIndex, 1);
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('hide');
    };

    $scope.detailPopUp = function (inveReveiveId) {
        $http.get($scope.path + 'GetInventoryMaterialList?inveReveiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.inventoryMaterialList = response.data.Rows;
                checkSameValueInColumnList($scope.inventoryMaterialList, 'TransactionUoM');
            });

        $http.get($scope.path + 'GetServiceChargeList?receiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.chargesList = [];
                $scope.chargesList = response.data;
            });
        angular.element(document.querySelector('#detailPopUp')).modal('show');
    }

    $scope.closeDetailPopUp = function () {
        $scope.inventoryMaterialList = [];

        angular.element(document.querySelector('#detailPopUp')).modal('hide');
    }
    function checkSameValueInColumnList(list, fieldName) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i][fieldName] === (i > 0 ? list[i - 1][fieldName] : list[i][fieldName]))
                $scope.sumORnot = true;
            else return $scope.sumORnot = false;
        }
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.Save = function () {
        try {
            //if (baseService.arrayLength($scope.modelList) === 0) return ShowResult('Select GRN', 'failure');
            $http({
                method: 'POST'
                , url: $scope.updateUrl
                , data: {
                    entities: $scope.modelList,
                    GRNStatus: $scope.GRN
                }
                , dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.getalldata1();
                   // $scope.Griddataapprovpo1();
                    $scope.getListForGRNUnchecked();

                }
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            throw e;
        }
    };
    $scope.SaveUpdate = function () {
        //debugger;
        if ($scope.podata.AuthorizedByStatus === null || $scope.podata.AuthorizedByStatus === "") {
            ShowResult("Please Select Approved By Status", 'failure');
            return false;
        }
        else if ($scope.podata.AuthorizedByStatus === "Checked" || $scope.podata.AuthorizedByStatus === "For Checked" || $scope.podata.CheckedStatus === "Select") {
            ShowResult("Please Select Approved By Status", 'failure');
            return false;
		}
		else if ($scope.podata.AuthorizedByStatus === "Hold" || $scope.podata.AuthorizedByStatus === "Reject") {
			if ($scope.podata.RejectApprovedReason === "" || $scope.podata.RejectApprovedReason === null || $scope.podata.RejectApprovedReason === undefined) {
				ShowResult("Enter The Reason", 'failure');
				return false;
			}

		}
        try {
            //if (baseService.arrayLength($scope.modelList) === 0) return ShowResult('Select GRN', 'failure');
			$http({				
				method: 'POST',
				url: $scope.updateUrl1
                , data: {
                    entities: $scope.modelList,
                    //GRNStatus: $scope.GRN,
                   // GRNNo: $scope.GRNNo,
					//AuthorizedByStatus: $scope.podata.AuthorizedByStatus,   
					//RejectApprovedReason: $scope.podata.RejectApprovedReason,   
                    GRNStatus: $scope.podata.AuthorizedByStatus,
                    GRNNo: $scope.podata.Id,
					AuthorizedByStatus: $scope.podata.AuthorizedByStatus,   
					RejectApprovedReason: $scope.podata.RejectApprovedReason,   
				},
				dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.LoadapprovalStatus();
                    $scope.getalldata1();
					$scope.Griddataapprovpo1();
					$scope.GRNNGriddataHoldReject();
                }
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            throw e;
        }
    };
    $scope.Clear = function () {
        $scope.modelList = [];
    };

    //Muntasir
    // #region setTab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    //#region KaziTaufik Code for GRN Approval
   
    $scope.onClickPO = function (args) {
        //debugger;
        var gridObj = $("#Grid").data("ejGrid");
        //getting corresponding record 
        $scope.data = gridObj.getSelectedRecords()[0];
        //alert('POClose' + data.Id);
        $scope.approveAlert();

    };
    $scope.Griddata1 = [];
    $scope.getalldata1 = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/GoodsReceiveNote/GetListForGRNApproval',
        }).then(function successCallback(response) {
            $scope.Griddata1 = response.data;
            //entrydata = copy(searchdata);
        });
    };
	$scope.getalldata1();


	$scope.GriddataHoldReject = [];
	$scope.GRNNGriddataHoldReject = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/GoodsReceiveNote/GetListForGRNApprovalHoldReject',
		}).then(function successCallback(response) {
			$scope.GriddataHoldReject = response.data;
			//entrydata = copy(searchdata);
		});
	};
	$scope.GRNNGriddataHoldReject();




    $scope.poApp = function () {
        $http({
            method: 'POST',
            url: 'Products/GoodsReceiveNote/PoApproved',
            data: {
                'PoId': $scope.podata.Id,
                'PoValue': $scope.podata.TotalQty
            },

            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getalldata1();
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }
    $scope.onClickPOA = function (args) {
		//debugger;
        var gridObj = $("#GridPO").data("ejGrid");
        //getting corresponding record 
        $scope.podata = gridObj.getSelectedRecords()[0];
        //alert('Approve=' + data.Id);
        $scope.GRNNo = $scope.podata.Id;
        $scope.GRN = 0;
        $scope.approvalAlert();
    };
    $scope.commandpo = [{
        type: "details", buttonOptions: {
            text: "Approve",
            width: "100",
            height: "30",

            click: $scope.onClickPOA
        }
	}];

	$scope.onClickPOAHR = function (args) {

		var gridObj = $("#GridGRNHR").data("ejGrid");
		//getting corresponding record 
		$scope.podata = gridObj.getSelectedRecords()[0];
		//alert('Approve=' + data.Id);
		$scope.GRNNo = $scope.podata.Id;
		$scope.GRN = 0;
		$scope.approvalAlert();
	};
	$scope.commandpoHR = [{
		type: "details", buttonOptions: {
			text: "Approve",
			width: "100",
			height: "30",
			click: $scope.onClickPOAHR
		}
	}];
    $scope.approvalAlert = function () {
        $scope.message = 'Are you sure want to Approve?';

        angular.element(document.querySelector('#poapprovealert')).modal('show');
    };


    //#endregion


//#region Grn Approval UI all code


    $scope.onClickPOA = function (args) {

        var gridObj = $("#GridPO").data("ejGrid");
        //getting corresponding record 
        $scope.podata = gridObj.getSelectedRecords()[0];
        //alert('Approve=' + data.Id);
        $scope.GRNNo = $scope.podata.Id;
        $scope.GRN = 0;
        $scope.approvalAlert();
    };
    $scope.commandpo = [{
        type: "details", buttonOptions: {
            text: "Approved",
            width: "100",
            height: "30",

            click: $scope.onClickPOA
        }
    }];
    $scope.approvalAlert = function () {
        $scope.message = 'Are you sure want to Approve?';
        angular.element(document.querySelector('#poapprovealert')).modal('show');
    };
    $scope.UncheckedAlert = function () {
        $scope.message = 'Are you sure want to Checked?';
        angular.element(document.querySelector('#poapprovealert')).modal('show');
    };
    $scope.onClickUnchecked = function (args) {
        //debugger;
        var gridObj = $("#GridPO").data("ejGrid");
        //getting corresponding record 
        $scope.podata = gridObj.getSelectedRecords()[0];
        //alert('Approve=' + data.Id);
        $scope.GRNNo = $scope.podata.Id;
        $scope.GRN = 0;
        $scope.UncheckedAlert();
    };
    $scope.commandpoUnchecked = [{
        type: "details", buttonOptions: {
            text: "Checked",
            width: "100",
            height: "30",

            click: $scope.onClickUnchecked
        }
    }];


    $scope.onClickUnchecked = function (args) {
        //debugger;
        var gridObj = $("#GridRejHold").data("ejGrid");
        //getting corresponding record 
        $scope.podata = gridObj.getSelectedRecords()[0];
        //alert('Approve=' + data.Id);
        $scope.GRNNo = $scope.podata.Id;
        $scope.GRN = 0;
        $scope.UncheckedAlert();
    };
    $scope.commandpoUncheckedHR = [{
        type: "details", buttonOptions: {
            text: "Checked",
            width: "100",
            height: "30",

            click: $scope.onClickUnchecked
        }
    }];

    //#endregion

 

    $scope.GRNCheckedBy = function () {
        try {
        var str1 = $('#combo-default1').val();
        var str = $scope.podata.SystemId;
        if ($scope.podata.CheckedStatus === "Hold" || $scope.podata.CheckedStatus === "Reject") {
        }
        else {
            if ($scope.podata.AuthorizedBy === "" || $scope.podata.AuthorizedBy === null || $scope.podata.AuthorizedBy === undefined) {
                ShowResult("Please Select To be Approved By", 'failure');
                return false;
            }
        }

        if ($scope.podata.CheckedStatus === null || $scope.podata.CheckedStatus === "") {
            ShowResult("Please Select Checked By Status", 'failure');
            return false;
        }
        else if ($scope.podata.CheckedStatus === "For Checked" || $scope.podata.CheckedStatus === "Select") {
            ShowResult("Please Select Checked By Status", 'failure');
            return false;
        }
        else if ($scope.podata.CheckedStatus === "Hold" || $scope.podata.CheckedStatus === "Reject") {
            if ($scope.podata.CheckedRejectReason === "" || $scope.podata.CheckedRejectReason === null || $scope.podata.CheckedRejectReason === undefined) {
                ShowResult("Enter The Reason", 'failure');
                return false;
            }

            }
      
            var filteredData = $scope.podata.Id;
            var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("InventoryReceiveId", "equal", parseInt(filteredData), true).take(200));
            if (data.length == 0) {
                throw "GRN Details is reuired.";
            }
        //debugger;
        $http({
            method: 'POST',
            url: 'Products/GoodsReceiveNote/GRNChecked',
            data: {
                'PoId': $scope.podata.Id,
                'PoValue': $scope.podata.TotalQty,
                'CheckedStataus': $scope.podata.CheckedStatus, //$('#combo-default').val(),
                'AuthorizedBy': $scope.podata.AuthorizedBy,
                'CheckedRejectReason': $scope.podata.CheckedRejectReason
            },

            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getListForGRNUnchecked();
                $scope.GetSupervisorCboList();
                $scope.poApproved();
                $scope.getListForGRNRejectHoldList();
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }


    // #region Taufik That is for Dropdown list
    $scope.checkedByList = [];
    $scope.GetSupervisorCboList = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/InventoryReceive/GetSupervisorCboApproved'
            // url: 'Products/InventoryCheckApproved/GetSupervisorCbo'
        }).then(function successCallback(response) {
            $scope.checkedByList = response.data;
        });
    }
    $scope.GetSupervisorCboList();




    $scope.poApproved = function () {
        cboService.getEnumCbo("enum/GetPOApprovalStatusCbo", function (result) {
            $scope.POApprovalList = result;
        });
    }
    $scope.poApproved();

     //#endregion



//#Endregion




   

   

    //$scope.GriddataGRNUnCheck = [];
    //$scope.Griddataapprovpo1 = function () {
    //    $http({
    //        method: "GET",
    //        dataType: 'JSON',
    //        //url: $scope.getSearchListUrl,
    //        url: 'Products/GoodsReceiveNote/GetListForGRNUnCheck',
    //    }).then(function successCallback(response) {
    //        $scope.GriddataGRNUnCheck = response.data;

    //        //entrydata = copy(searchdata);
    //    });
    //};
    //$scope.Griddataapprovpo1();


    $scope.onClickPOA1 = function (args) {
        //debugger;
        var gridObj = $("#GridPO1").data("ejGrid");
        //getting corresponding record 
        $scope.podata1 = gridObj.getSelectedRecords()[0];
        $scope.GRNNo = $scope.podata1.Id;
        $scope.GRN = 1;
        $scope.approveAlert1();
    };
    $scope.approveAlert1 = function () {
        $scope.message = 'Are you sure want to Approve?';
        angular.element(document.querySelector('#poapprovalalert1')).modal('show');
    };
    $scope.commandUnC= [{
        type: "details", buttonOptions: {
            text: "Un Check",
            width: "100",
            height: "30",

            click: $scope.onClickPOA1
        }
    }];

    $scope.poApp1 = function () {
        $http({
            method: 'POST',
            url: 'Products/GoodsReceiveNote/GRNUnCheck',
            data: {
                'PoId': $scope.podata1.Id,
                'PoValue': $scope.podata1.TotalQty
            },

            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.Griddataapprovpo1();
                $scope.getListForGRNUnCheck();
                
        
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }
    //#endregion






    // #region Taufik Un Approval GRN data post start
    $scope.Griddataapprovpo = [];
    $scope.Griddataapprovpo1 = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/GoodsReceiveNote/GetListForGRNUNApproval',
        }).then(function successCallback(response) {
            $scope.Griddataapprovpo = response.data;

            //entrydata = copy(searchdata);
        });
    };
    $scope.Griddataapprovpo1();

   
    $scope.onClickPOA1 = function (args) {
        //debugger;
        var gridObj = $("#GridPO1").data("ejGrid");
        //getting corresponding record 
        $scope.podata1 = gridObj.getSelectedRecords()[0];        
        $scope.GRNNo = $scope.podata1.Id;
       $scope.GRN =1;
        $scope.approveAlert1();
    };
    $scope.approveAlert1 = function () {
        $scope.message = 'Are you sure want to Approve?';
        angular.element(document.querySelector('#poapprovalalert1')).modal('show');
    };
    $scope.commandpo1 = [{
        type: "details", buttonOptions: {
            text: "Un Approve",
            width: "100",
            height: "30",

            click: $scope.onClickPOA1
        }
    }];

    $scope.poApp1 = function () {
        $http({
            method: 'POST',
            url: 'Products/GoodsReceiveNote/PoApproved1',
            data: {
                'PoId': $scope.podata1.Id,
                'PoValue': $scope.podata1.TotalQty
            },

            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.Griddataapprovpo1();
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }
   

    $scope.LoadapprovalStatus = function () {
        cboService.getEnumCbo("enum/GetCheckedStatusCbo", function (result) {
            $scope.approvalStatusList = result;
        });

    }
    $scope.LoadapprovalStatus();



    // #region All Tab Control
    $scope.GRN = "";
    $scope.tab = 1;
    $scope.setTabpou = function (newTab) {
        $scope.tab = newTab;   
		$scope.getListForGRNUnchecked();
		
    };
    $scope.isSetpou = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 0;
	};


	$scope.setTabpou1 = function (newTab) {
		$scope.tab = newTab;		
		$scope.getalldata1();
	};
	$scope.isSetpou1 = function (tabNum) {
		return $scope.tab === tabNum;
		$scope.GRN = 0;
	};
   // $scope.tab = 2;

    $scope.setTabpoa = function (newTab) {
      
        $scope.tab = newTab;
        $scope.GRN = 1;
		$scope.getListForGRNCheckedList();
        //alert('Checked Tab');
    };
    $scope.isSetpoa = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 1;
    };
    // End PO approve

	$scope.setTabGRNRejectHoldList = function (newTab) {

		$scope.tab = newTab;
		//$scope.getListForGRNRejectHoldList();
		$scope.getListForGRNRejectHoldList();
	};
	$scope.isSetGRNRejectHoldList = function (tabNum) {
		return $scope.tab === tabNum;
		
	};

	$scope.setTabGRNRejectHoldList1 = function (newTab) {

		$scope.tab = newTab;
		//$scope.getListForGRNRejectHoldList();
		$scope.GRNNGriddataHoldReject();
	};
	$scope.isSetGRNRejectHoldList1 = function (tabNum) {
		return $scope.tab === tabNum;

	};

    $scope.setTabpoApproval = function (newTab) {

        $scope.tab = newTab;
        $scope.GRN = 1;
        $scope.Griddataapprovpo1();

        //alert('Checked Approval');
    };
    $scope.isSetpoApproval = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 1;
	};




	$scope.setTabGRNApproval = function (newTab) {

		$scope.tab = newTab;
		$scope.GRN = 1;
		$scope.Griddataapprovpo1();

		//alert('Checked Approval');
	};
	$scope.isSetGRNApproval = function (tabNum) {
		return $scope.tab === tabNum;
		$scope.GRN = 1;
	};
    

// #endregion

    //#region Print for po Approval

    $scope.onClickpoApprovalprint = function (args) {

        var gridObj = $("#GridPO1").data("ejGrid");
        //getting corresponding record             
        var data = gridObj.getSelectedRecords()[0];
        //alert('jj' + data.Id);
        // $scope.valuePassInDelModal(data); 
        location.href = "Products/PurchaseOrder/GePurchaseOrderReport?purchaseOrderId=" + data.Id;

    };
    $scope.commandprint = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",

            click: $scope.onClickpoApprovalprint
        }
    }];

    //#endregion


    // #endregion setTab


    $scope.onClickReportDownloadWord = function (args) {
        //debugger;
        var gridObj = $("#GridPO1").data("ejGrid");
        //getting corresponding record 
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        //$window.open('GoodsReceiveNote/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.Id + '&plantId=' + $scope.productNew.PlantId);
        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };

    $scope.commandWord = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",
            click: $scope.onClickReportDownloadWord
        }
    }];



   
	//$window.onresize = function (event) {

	//	$scope.GRNUnApprovalScroll1();

	//};
	//$scope.GRNUnApprovalScroll1 = function (args) {
	//	try {
	//		if (args.requestType === "refresh") {
	//			var gridObj = $("#GridPO1").ejGrid("instance");
	//			var scrollerwidth = $("#GRNApproval").width();//Obtain the width of the container

	//			//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
	//			gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
	//			gridObj.windowonresize();
	//		}
	//	} catch (e) {
	//		//$scope.ShowResultCustom(e, 'failure');
	//	}
	//};







    $window.onresize = function (event) {

        $scope.GRNUncheckedScroll();

    };
    $scope.GRNUncheckedScroll = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridPO").ejGrid("instance");
                var scrollerwidth = $("#GRNUncheck").width();//Obtain the width of the container

                //   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };
    $window.onresize = function (event) {

        $scope.GRNcheckedScroll();

    };
    $scope.GRNcheckedScroll = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridPO1").ejGrid("instance");
                var scrollerwidth = $("#GRNCheck").width();//Obtain the width of the container

                //   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
	};
	$window.onresize = function (event) {

		$scope.GRNcheckedHoldRejectScroll();

	};
	$scope.GRNcheckedHoldRejectScroll = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#GridRejHold").ejGrid("instance");
				var scrollerwidth = $("#GRNUncheck1").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};



	//#region GRN Approval Tab
	$scope.tabapp = 1;
	$scope.setTabUnApprovedGRnList = function (newTab) {

		$scope.tabapp = newTab;		
		$scope.getalldata1();
	};
	$scope.isSetUnApprovedGRnList = function (tabNum) {
		return $scope.tabapp === tabNum;

	};
	
	$scope.setTabUnApprovedGRnListHold = function (newTab) {

		$scope.tabapp = newTab;
		//$scope.getListForGRNRejectHoldList();
		$scope.GRNNGriddataHoldReject();
	};
	$scope.isSetUnApprovedGRnListHold = function (tabNum) {
		return $scope.tabapp === tabNum;

	};
	$scope.setTabApprovedGRnListHold = function (newTab) {

		$scope.tabapp = newTab;
		//$scope.getListForGRNRejectHoldList();
		$scope.Griddataapprovpo1();
	};
	$scope.isSetApprovedGRnListHold = function (tabNum) {
		return $scope.tabapp === tabNum;

	};



	$window.onresize = function (event) {

		$scope.GRNUnApprovalScrollHold();

	};
	$scope.GRNUnApprovalScrollHold = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#Sk1").ejGrid("instance");
				var scrollerwidth = $("#GridGRNHR").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};



	$window.onresize = function (event) {

		$scope.GRNUnApprovalScroll();

	};
	$scope.GRNUnApprovalScroll = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#GridPO").ejGrid("instance");
				var scrollerwidth = $("#GRNUnApproval").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};



	$window.onresize = function (event) {

		$scope.GRNApprovalScroll();

	};
	$scope.GRNApprovalScroll = function (args) {
		//debugger;
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#GridPO1").ejGrid("instance");
				var scrollerwidth = $("#GRNApproval").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};
	//#endregion




     //#endregion

   


    //#region GRN Check UI screen

    $scope.AllTabPrint = function (z) {
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "GoodsReceiveNote/GRNReport?grnId=" + data.Id + '&plantId=' + data.PlantId;



    };

    $scope.GriddataGRNCk = [];
    $scope.getListForGRNUnchecked = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/GoodsReceiveNote/getListForGRNUnchecked',
        }).then(function successCallback(response) {
            $scope.GriddataGRNCk = response.data;

        });
    };
    $scope.getListForGRNUnchecked();



    $scope.GriddataGRNChecked = [];
    $scope.getListForGRNCheckedList = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/GoodsReceiveNote/getListForGRNChecked',
        }).then(function successCallback(response) {
            $scope.GriddataGRNChecked = response.data;

        });
    };
    $scope.getListForGRNCheckedList();



    $scope.GriddataGRNRejectHoldList = [];
    $scope.getListForGRNRejectHoldList = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/GoodsReceiveNote/getListForGRNRejectHoldList',
        }).then(function successCallback(response) {
            $scope.GriddataGRNRejectHoldList = response.data;

        });
    };
    $scope.getListForGRNRejectHoldList();

    $scope.onClickPO = function (args) {
        //debugger;
        var gridObj = $("#Grid").data("ejGrid");
        //getting corresponding record 
        $scope.data = gridObj.getSelectedRecords()[0];
        //alert('POClose' + data.Id);
        $scope.approveAlert();

    };

    $scope.poApp = function () {
        $http({
            method: 'POST',
            url: 'Products/GoodsReceiveNote/GRNCheck',
            data: {
                'PoId': $scope.podata.Id,
                'PoValue': $scope.podata.TotalQty
            },

            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getalldata1();
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.onClickSave = function (z) {
        debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        //var gridObj = $("#GridPO").data("ejGrid");
        //getting corresponding record 
        $scope.podata = gridObj.getSelectedRecords()[0];
        //alert('Approve=' + data.Id);
        // $scope.approvalAlert();
        $scope.url = $location.absUrl().split('!/')[1]
        if ($scope.url === 'grn-approval') {
            //$scope.tabType = 'UnApprovedList';
            if ($scope.podata.AuthorizedByStatus === 'Approval') {
                $scope.POPUpStatus = 'Approve';

            }
            else if ($scope.podata.AuthorizedByStatus === 'For Approval' || $scope.podata.AuthorizedByStatus === 'Select') {
                ShowResult('Select Other Status', 'failure');
                return false;

            }
           
            else if ($scope.podata.AuthorizedByStatus === 'Hold' && baseService.isUndefinedOrNull($scope.podata.RejectApprovedReason)) {
                ShowResult('Enter the reason', 'failure');
                return false;

            }
            else if ($scope.podata.AuthorizedByStatus === 'Reject' && baseService.isUndefinedOrNull($scope.podata.RejectApprovedReason)) {
                ShowResult('Enter the reason', 'failure');
                return false;

            }
            else {
                $scope.POPUpStatus = $scope.podata.AuthorizedByStatus;

            }
        }
        else if ($scope.url === 'Grn-Check') {
            // $scope.tabType = 'UnCheckedList';
            if ($scope.podata.CheckedStatus === 'Checked') {
                $scope.POPUpStatus = 'Check';
                if (baseService.isUndefinedOrNull($scope.podata.AuthorizedBy)) {
                    ShowResult('Select to be approved by', 'failure');
                    return false;

                }
            }
            else if (baseService.isUndefinedOrNull($scope.podata.CheckedStatus) || $scope.podata.CheckedStatus === 'Select') {
                ShowResult('Select Other Status', 'failure');
                return false;

            }
            else if ($scope.podata.CheckedStatus === 'Hold' && baseService.isUndefinedOrNull($scope.podata.CheckedRejectReason)) {
                ShowResult('Enter the reason', 'failure');
                return false;

            }
            else if ($scope.podata.CheckedStatus === 'Reject' && baseService.isUndefinedOrNull($scope.podata.CheckedRejectReason)) {
                ShowResult('Enter the reason', 'failure');
                return false;

            }

            else {
                $scope.POPUpStatus = $scope.podata.CheckedStatus;

            }
        }
        $scope.message = 'Are you sure to ' + $scope.POPUpStatus + '?';
        angular.element(document.querySelector('#poapprovealert')).modal('show');
    };
    $scope.RecentApprovedData = [];
    $scope.GetRecentApprovedData = function (z) {
        $scope.RecentApprovedData = [];
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $http({
            method: 'GET',
            url: 'Products/GoodsReceiveNote/GetRecentApprovedData?grnId=' + data.Id
        }).then(function (response) {
            $scope.RecentApprovedData = response.data;
        });
        angular.element(document.querySelector('#PopUpRecentApprovedData')).modal('show');
    }
    $scope.CloseRecentApprovedDataPopUp = function () {
        angular.element(document.querySelector('#PopUpRecentApprovedData')).modal('hide');

    }


    //#endregion
}