'use strict';
ServiceRequisitionCheckApprovedController.$inject = ['$window', 'cboService', '$scope', '$rootScope', '$http','baseService'];
function ServiceRequisitionCheckApprovedController($window, cboService, $scope, $rootScope, $http, baseService) {
    $rootScope.title = "Service Requisition Check Approved";
    $scope.Action = 'Save'; 
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Products/InventoryCheckApproved/';
	$scope.path1 = 'Products/Requisition/';
	$scope.detailUpdate = 'Products/Requisition/UpdateApprovedQty';

    $scope.AllTabPrint = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/ServiceRequisition/ServiceRequisitionReportby?RequisitionId=" + data.Id;

    }; 


//EndRegion Old code

    //#region Requisition 
    $scope.LoadapprovalStatus = function () {
        cboService.getEnumCbo("enum/GetCheckedStatusCbo", function (result) {
            $scope.approvalStatusList = result;
        });

    }
    $scope.LoadapprovalStatus();
    
      //#region Requijition hold / Reject TAB In index

    $scope.RequisitionHoldRejectList = [];
    $scope.RequisitionHoldReject = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/ServiceRequisitionCheckApproved/GetListRequisionHoldReject',
        }).then(function successCallback(response) {
            $scope.RequisitionHoldRejectList = response.data;
            //entrydata = copy(searchdata);
        });
    };
    $scope.RequisitionHoldReject();

   

    //endregion

    // #region  Tab Control for  Service Requisition Check
    $scope.tab = 1;
    $scope.setTabReqUnchecked = function (newTab) {
        $scope.tab = newTab;
        $scope.RequisitionUnchecked();
        

    };
    $scope.isSetReqUnchecked = function (tabNum) {
        return $scope.tab === tabNum;
    };  

    $scope.setTabReqHoldRreject = function (newTab) {
        $scope.tab = newTab;
        $scope.RequisitionHoldReject();
        

    };
    $scope.isSetReqHoldReject = function (tabNum) {
        return $scope.tab === tabNum;
    };



    $scope.setTabReqchecked = function (newTab) {
        $scope.tab = newTab;
        $scope.Requisitionchecked();
        //$scope.RequisitionHoldReject();

    };
    $scope.isSetReqchecked = function (tabNum) {
        return $scope.tab === tabNum;
    };

    //#endregion Service Requisition Check
      // #region  Tab Control for  Service Requisition Approve
    $scope.setTabReqUnApproved = function (newTab) {
        $scope.tab = newTab;
		$scope.RequisitionUnapproved();
        //$scope.Requisitionchecked();
    };
    $scope.isSetReqUnApproved = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTabApprovedReqHoldReject = function (newTab) {
        $scope.tab = newTab;
        $scope.ApprovedRequisitonHoldReject();

    };
    $scope.isSetApprovedReqHoldReject = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTabReqapproved = function (newTab) {
        $scope.tab = newTab;
        
		$scope.Requisitionapproved();
    };
    $scope.isSetReqapproved = function (tabNum) {
        return $scope.tab === tabNum;
    };

// #endregion Service Requisition Approve

    //#Region Grid data bind 
    $scope.RequisitionUncheckedList = [];
    $scope.RequisitionUnchecked = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/ServiceRequisitionCheckApproved/GetListRequisionUnchecked',
        }).then(function successCallback(response) {
            $scope.RequisitionUncheckedList = response.data;
        });
    };
    $scope.RequisitionUnchecked();

    $scope.RequisitioncheckedList = [];
    $scope.Requisitionchecked = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/ServiceRequisitionCheckApproved/GetListRequisionchecked',
        }).then(function successCallback(response) {
            $scope.RequisitioncheckedList = response.data;

            //entrydata = copy(searchdata);
        });
    };
    $scope.Requisitionchecked();

    $scope.ReqUnapprovedList = [];
    $scope.RequisitionUnapproved = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',           
            url: 'Products/ServiceRequisitionCheckApproved/GetListRequisionUnApproved',
        }).then(function successCallback(response) {
            $scope.ReqUnapprovedList = response.data;
        });
    };
    $scope.RequisitionUnapproved();

    $scope.ApprovedReqHRList = [];
    $scope.ApprovedRequisitonHoldReject = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/ServiceRequisitionCheckApproved/GetListRequisionApprovedHoldReject',
        }).then(function successCallback(response) {
            $scope.ApprovedReqHRList = response.data;
        });
    };
    $scope.ApprovedRequisitonHoldReject();

    $scope.RequisitionapprovedList = [];
    $scope.Requisitionapproved = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/ServiceRequisitionCheckApproved/GetListRequisionApproved',
        }).then(function successCallback(response) {
            $scope.RequisitionapprovedList = response.data;
        });
    };
    $scope.Requisitionapproved();

    $scope.checkedByList = [];
    $scope.GetSupervisorCboList = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/InventoryCheckApproved/GetSupervisorCbo'
        }).then(function successCallback(response) {
            $scope.checkedByList = response.data;
        });
    }
    $scope.GetSupervisorCboList();

    $scope.checkedByList1 = [];
    //debugger;
    $scope.GetSupervisorCboList1 = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/ServiceRequisitionCheckApproved/GetSupervisorCboApproved'
        }).then(function successCallback(response) {
            $scope.checkedByList1 = response.data;
        });
    }
    $scope.GetSupervisorCboList1();
    $scope.poApproved = function () {
        cboService.getEnumCbo("enum/GetPOApprovalStatusCbo", function (result) {
            $scope.POApprovalList = result;
        });
    }
    $scope.poApproved();
    //$scope.onClickSPOCheck = function (z) {
    //    //debugger;
    //    var x = "#" + z;
    //    var gridObj = $(x).data("ejGrid");
    //    $scope.podata = gridObj.getSelectedRecords()[0];
    //    //alert('Approve=' + data.Id);
    //    $scope.message = 'Are you sure want to ' + $scope.podata.CheckedStatus + '?';
    //    //$scope.message = 'Are you sure want to Hold or Reject or Checked ?' + $scope.podata.CheckedStatus;
    //    angular.element(document.querySelector('#poapprovealert')).modal('show');
    //    //$scope.CheckAlert();
    //};

    $scope.onClickSPOCheck = function (z) {
        debugger;

        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        $scope.podata = gridObj.getSelectedRecords()[0];
        $scope.message = 'Are you sure want to ' + $scope.podata.CheckedStatus + '?';
        angular.element(document.querySelector('#poapprovealert')).modal('show');

    };
    //debugger;
    $scope.commandpo = [{
        type: "details", buttonOptions: {
            text: "Save",
            width: "100",
            height: "30",
            click: $scope.onClickSPOCheck
        }
    }];

    $scope.CheckAlert = function () {
        $scope.message = 'Are you sure want to Hold or Reject or Checked ?';
        angular.element(document.querySelector('#poapprovealert')).modal('show');
    };


    $scope.onClickRequisitionHoldReject = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        $scope.podata = gridObj.getSelectedRecords()[0];
        //alert('Approve=' + data.Id);
        //$scope.HoldandRejectAlert();
        $scope.message = 'Are you sure want to ' + $scope.podata.CheckedStatus + '?';
        angular.element(document.querySelector('#poapprovealert')).modal('show');
    };
    //debugger;
    $scope.commandpo = [{
        type: "details", buttonOptions: {
            text: "Save",
            width: "100",
            height: "30",
            click: $scope.onClickRequisitionHoldReject
        }
    }];

    $scope.HoldandRejectAlert = function () {
        $scope.message = 'Are you sure want to Checked?';
        angular.element(document.querySelector('#poapprovealert')).modal('show');
    };
  
    $scope.onClickSPOApproved = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        $scope.podata = gridObj.getSelectedRecords()[0];
        //alert('Approve=' + data.Id);
       // $scope.approvalSpoAlert();
        $scope.message = 'Are you sure want to ' + $scope.podata.CheckedStatus + '?';
        angular.element(document.querySelector('#poapprovealert')).modal('show');
    };
    //debugger;
    $scope.commandpo = [{
        type: "details", buttonOptions: {
            text: "Save",
            width: "100",
            height: "30",
            click: $scope.onClickSPOApproved
        }
    }];

    $scope.approvalSpoAlert = function () {
        $scope.message = 'Are you sure want to Hold or Reject or Approval ?';
        angular.element(document.querySelector('#poapprovealert')).modal('show');
    };

    $scope.onClickServiceReqA = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        $scope.podata = gridObj.getSelectedRecords()[0];
        //alert('Approve=' + data.Id);
        //$scope.approvalAlert();
       // if ($scope.podata.CheckedStatus === 'Approval')
        $scope.message = 'Are you sure want to ' + $scope.podata.CheckedStatus + '?';
        angular.element(document.querySelector('#poapprovealert')).modal('show');
    };
    //debugger;
    $scope.commandpo = [{
        type: "details", buttonOptions: {
            text: "Save",
            width: "100",
            height: "30",
            click: $scope.onClickPOA
        }
    }];

    $scope.approvalAlert = function () {
        $scope.message = 'Are you sure want Approval ?';
        angular.element(document.querySelector('#poapprovealert')).modal('show');
    };

  

 

    $scope.onClickPOAUTH = function (z) {

        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        $scope.podata = gridObj.getSelectedRecords()[0];
        $scope.approvalAlert();
    };


    $scope.onClickReqAHR = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        $scope.podata = gridObj.getSelectedRecords()[0];
        $scope.approvalAlert();
    };

	//$scope.poApp = function () {
	//	//debugger;		
		
	//	if ($scope.podata.CheckedStatus === "Hold" || $scope.podata.CheckedStatus === "Reject") {

	//	}
 //       else {
 //           if ($scope.podata.AuthorizedBy === "" || $scope.podata.AuthorizedBy === null || $scope.podata.AuthorizedBy === undefined) {
	//			ShowResult("Please Select To be Approved By", 'failure');
	//			return false;
	//		} 
           
	//	}	
		
	//	if ($scope.podata.CheckedStatus === null || $scope.podata.CheckedStatus === "") {
	//		ShowResult("Please Select Checked By Status", 'failure');
	//		return false;
	//	}
	//	else if ($scope.podata.CheckedStatus === "For Checked" || $scope.podata.CheckedStatus === "Select") {
	//		ShowResult("Please Select Checked By Status", 'failure');
	//		return false;
	//	}
	//	else if ($scope.podata.CheckedStatus === "Hold" || $scope.podata.CheckedStatus === "Reject") {
	//		if ($scope.podata.CheckedRejectReason === "" || $scope.podata.CheckedRejectReason === null || $scope.podata.CheckedRejectReason === undefined) {
	//			ShowResult("Enter The Reason", 'failure');
	//			return false;
	//		}
			
	//	}
       
 //       $http({
 //           method: 'POST',
 //           url: 'Products/ServiceRequisitionCheckApproved/ReqChecked',
 //           data: {
 //               'SRMId': $scope.podata.Id,
 //               'PoValue': $scope.podata.TotalQty,
 //               'CheckedStataus': $scope.podata.CheckedStatus,
	//			'CheckedHoldRejectReason': $scope.podata.CheckedRejectReason,
 //               'AuthorizedBy': $scope.podata.AuthorizedBy,
 //               'RequisitionType': $scope.podata.RequisitionType,
 //               'RequirmentType': $scope.podata.RequirmentType,
 //               'CheckedBy': $scope.podata.CheckedBy,
 //               'PreparedBY': $scope.podata.AddedBy
 //           },

 //           dataType: 'JSON'
 //       }).then(function successCallback(response) {
 //           if (response.data.Error === true) {
 //               ShowResult(response.data.Message, 'failure');
 //           }
 //           else {
 //               ShowResult(response.data.Message, 'success');
 //               $scope.RequisitionUnchecked();
 //               $scope.GetSupervisorCboList();
 //               $scope.poApproved();

 //           }
 //       }, function errorCallBack(response) {
 //           ShowResult(response.data.Message, 'failure');
 //       });
 //   }

    $scope.poApp = function () {
        try {

            if ($scope.podata.CheckedStatus === "For Checked" || $scope.podata.CheckedStatus === "Select" || baseService.isUndefinedOrNull($scope.podata.CheckedStatus)) {
                ShowResult("Please Select Checked By Status", 'failure');
                return false;
            }

            else if ($scope.podata.CheckedStatus === "Checked" && baseService.isUndefinedOrNull($scope.podata.AuthorizedBy)) {
                ShowResult("Please Select To be Approved By", 'failure');
                return false;
            }
            else if (($scope.podata.CheckedStatus === "Hold" || $scope.podata.CheckedStatus === "Reject") && baseService.isUndefinedOrNull($scope.podata.CheckedRejectReason)) {
                ShowResult("Enter The Reason", 'failure');
                return false;
            }

            var filteredData = $scope.podata.Id;
            var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("ServiceRequisitionMasterID", "equal", parseInt(filteredData), true).take(100));
            if (data.length == 0) {
                throw "Service Requisition Details is Reuired.";
            }


            $http({
                method: 'POST',
                url: 'Products/ServiceRequisitionCheckApproved/ReqChecked',
                data: {
                    'SRMId': $scope.podata.Id,
                    'PoValue': $scope.podata.TotalQty,
                    'CheckedStataus': $scope.podata.CheckedStatus,
                    'CheckedHoldRejectReason': $scope.podata.CheckedRejectReason,
                    'AuthorizedBy': $scope.podata.AuthorizedBy,
                    'AuthorizedByStatus': $scope.podata.AuthorizedByStatus,
                    'RequisitionType': $scope.podata.RequisitionType,
                    'RequirmentType': $scope.podata.RequirmentType,
                    'CheckedBy': $scope.podata.CheckedBy,
                    'PreparedBY': $scope.podata.AddedBy

                },

                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.RequisitionUnchecked();
                    $scope.RequisitionHoldReject();
                    $scope.Requisitionchecked();
                    $scope.GetSupervisorCboList();
                    $scope.poApproved();


                }
            }, function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
  //  $scope.poAppAuth = function () {
  //      //debugger;
		//if ($scope.podata.CheckedStatus === null || $scope.podata.CheckedStatus === "") {
		//	ShowResult("Please Select Checked By Status", 'failure');
		//	return false;
		//}
		//else if ($scope.podata.CheckedStatus === "Checked" || $scope.podata.CheckedStatus === "For Checked" || $scope.podata.CheckedStatus === "Select") {
		//	ShowResult("Please Select Approved By Status", 'failure');
		//	return false;
		//}
		//else if ($scope.podata.CheckedStatus === "Hold" || $scope.podata.CheckedStatus === "Reject") {
		//	if ($scope.podata.RejectApprovedReason === "" || $scope.podata.RejectApprovedReason === null || $scope.podata.RejectApprovedReason === undefined) {
		//		ShowResult("Enter The Reason", 'failure');
		//		return false;
		//	}

		//}

  //      //debugger;
  //      $http({
  //          method: 'POST',
  //          url: 'Products/ServiceRequisitionCheckApproved/ReqApprovedAuth',
  //          data: {
  //              'SRMID': $scope.podata.Id,
  //              'PoValue': $scope.podata.TotalQty,
		//		'CheckedStataus': $scope.podata.CheckedStatus,
		//		'RejectApprovedReason': $scope.podata.RejectApprovedReason
  //          },

  //          dataType: 'JSON'
  //      }).then(function successCallback(response) {
  //          if (response.data.Error === true) {
  //              ShowResult(response.data.Message, 'failure');
  //          }
  //          else {
  //              ShowResult(response.data.Message, 'success');
  //              $scope.RequisitionUnapproved();
  //              $scope.LoadapprovalStatus();
  //          }
  //      }, function errorCallBack(response) {
  //          ShowResult(response.data.Message, 'failure');
  //      });
  //  }
    $scope.poAppAuth = function () {

        if (baseService.isUndefinedOrNull($scope.podata.CheckedStatus) || $scope.podata.CheckedStatus === "Select") {
            ShowResult("Please Select Approved By Status", 'failure');
            return false;
        }
        else if ($scope.podata.CheckedStatus === "Select" || $scope.podata.CheckedStatus === "For Approval") {
            ShowResult("Please Select Approved By Status", 'failure');
            return false;
        }
        else if (($scope.podata.CheckedStatus === "Hold" || $scope.podata.CheckedStatus === "Reject") && baseService.isUndefinedOrNull($scope.podata.RejectApprovedReason)) {

            ShowResult("Enter The Reason", 'failure');
            return false;
        }

        //var filteredData = $scope.podata.Id;
        //var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("MaterialReqqusitionMasterId", "equal", parseInt(filteredData), true).take(100));
        //if (data.length == 0) {
        //    throw "Requisition Details is reuired.";
        //}


        //debugger;
        $http({
            method: 'POST',
            url: 'Products/ServiceRequisitionCheckApproved/ReqApprovedAuth',
            data: {
                'SRMID': $scope.podata.Id,
                'PoValue': 0, //$scope.podata.TotalQty,
                'CheckedStataus': $scope.podata.CheckedStatus,
                'RejectApprovedReason': $scope.podata.RejectApprovedReason,
                'AuthorizedBy': $scope.podata.AuthorizedBy,
                'RequisitionType': $scope.podata.RequisitionType,
                'RequirmentType': $scope.podata.RequirmentType,
                'CheckedBy': $scope.podata.CheckedBy,
                'PreparedBY': $scope.podata.AddedBy,
                'PreparedBYId': $scope.podata.PreparedById
            },

            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.RequisitionUnapproved();
                $scope.ApprovedRequisitonHoldReject();
                $scope.Requisitionapproved();
                $scope.LoadapprovalStatus();
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }
    $scope.poAppUnApproved = function () {

        //debugger;
        $http({
            method: 'POST',
            url: 'Products/InventoryCheckApproved/PoUnApproved',
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
                $scope.getalldata1();
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    // $scope.valuePassInDelModal(data); 

    //#region  Req Detail
    $scope.lst = [];
    $scope.ReqListDetails = function () {
        //debugger;
        $http({
            method: 'GET',
            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
            url: 'Products/ServiceRequisition/GetAllServiceReqdataDetails'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            //$scope.detailgrid($scope.lst);
            window.lst = response.data;

        });
    }
    $scope.ReqListDetails();

    $scope.data1 = $scope.lst;
    $scope.detailTemp = "#tabGridContents";
    //$scope.detailgrid = "detailGridData(e)";
    $scope.detailgrid = function detailGridData(e) {
        //debugger;

        var filteredData = e.data["Id"];
        var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("ServiceRequisitionMasterID", "equal", parseInt(filteredData), true).take(100));
        e.detailsElement.find("#detailGrid").ejGrid({

			dataSource: data,
			//columns: ["MaterialGroupName", "MaterialName", "ArticleName", "SKU1", "SKU2", "SKU3","MaterialDetail", "TransactionQty", "TransactionUoMId", "TransactionUoM", "EstimatedRate", "CurrencyName", "TotalAmount"]
			
            columns: [{ field: "ServiceMasterName", headerText: "ServiceMasterName", width: 50 },
                { field: "CurrencyName", headerText: "CurrencyName", width: 150 },
                { field: "Rate", headerText: "ToCurrencyRate", width: 100 },
                { field: "TotalServiceTranAmount", headerText: "Amount(TRN)", width: 150 },
                { field: "TotalServiceBooksCurrencyAmount", headerText: "Amount(BC)", width: 150 },
			
			]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }

    //#endregion
      // #endregion Requisition
	$scope.recorddoubleclick = function ($event) {
		//debugger;
		var x = $event;
		var Id = x.data.Id;
		getInventoryMaterialList(Id);
	};
	function getInventoryMaterialList(ReqId) {

		//debugger;
		$scope.requisitionMaterialList = [];
		$http.get($scope.path1 + 'GetAllReqdataDetailsById?Id=' + ReqId)
			.then(function (response) {
				$scope.requisitionMaterialList = response.data;

			});
		angular.element(document.querySelector('#ListOfRequisition')).modal('show');

	}
	$scope.recorddoubleclick1 = function ($event) {
		//debugger;
		var x = $event;
		var Id = x.data.Id;
		getInventoryMaterialList1(Id);
	};

	function getInventoryMaterialList1(ReqId) {
		
		//debugger;
		$scope.requisitionMaterialList1 = [];
		$http.get($scope.path1 + 'GetAllReqdataDetailsById?Id=' + ReqId)
			.then(function (response) {
				$scope.requisitionMaterialList1 = response.data;
			
			});
		angular.element(document.querySelector('#ListOfRequisition1')).modal('show');

	}
	$scope.RequisitionListHide = function () {
		$scope.taxCategoryList = [];
		angular.element(document.querySelector('#ListOfRequisition')).modal('hide');
	}; 
	$scope.RequisitionListHide1 = function () {
		$scope.taxCategoryList = [];
		angular.element(document.querySelector('#ListOfRequisition1')).modal('hide');
	};  

	$scope.calculateAmount = function (data) {
		//debugger;
		angular.forEach($scope.requisitionMaterialList, function (item) {
			if (item.Id === data.Id) {
				if (data.ApprovedQty === undefined) {
					//data.ApprovedQty = 0;
					item.TotalAmount = data.TotalAmount;
				}
				else {
					item.TotalAmount = (data.ApprovedQty * data.EstimatedRate).toFixed(2);
				}
			}

		});	
	};

	$scope.UpdateQty = function () {
		//if ($scope.ApprovedQty === null || $scope.ApprovedQty === "") {
		//	ShowResult(response.data.Message, 'failure', 'ListOfRequisition');
		//}
			$http({
				method: 'POST',
				url: $scope.detailUpdate,
				data: { entity: $scope.requisitionMaterialList },
				dataType: 'JSON'
			}).then(function successCallback(response) {
				if (response.data.Error === true)
					ShowResult(response.data.Message, 'failure', 'ListOfRequisition');
				else {
					ShowResult(response.data.Message, 'success', 'ListOfRequisition');					

					$scope.ReqListDetails();
					$scope.RequisitionUnapproved();
					$scope.Requisitionapproved();
			
					//$scope.clearCharNames();
					
				}
			}), function errorCallBack(response) {
				ShowResult(response.data.Message, 'failure', 'ListOfRequisition');
			};
		
	}

    $scope.onClickAReqHR = function (args) {

        var gridObj = $("#GridReqAHR").data("ejGrid");
        //getting corresponding record             
        var data = gridObj.getSelectedRecords()[0];
        //alert('jj' + data.Id);
        // $scope.valuePassInDelModal(data); 
        location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.Id;

    };
    $scope.commandAReqHRPrint = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "100",
            height: "30",
            click: $scope.onClickAReqHR
        }
    }];

	$scope.onClickReqApp = function (args) {

        var gridObj = $("#GridPOAPp").data("ejGrid");
		//getting corresponding record             
		var data = gridObj.getSelectedRecords()[0];
		//alert('jj' + data.Id);
		// $scope.valuePassInDelModal(data); 
		location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.Id;

	};
    $scope.commandReqApprint = [{
		type: "details", buttonOptions: {
			text: "Print",
			width: "100",
			height: "30",
            click: $scope.onClickReqApp
		}
	}];

	//#region All Print option of Requisition Check
	$scope.onClick11 = function (args) {

		var gridObj = $("#GridPO").data("ejGrid");
		//getting corresponding record             
		var data = gridObj.getSelectedRecords()[0];
		//alert('jj' + data.Id);
		// $scope.valuePassInDelModal(data); 
		location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.Id;

	};
	$scope.command1 = [{
		type: "details", buttonOptions: {
			text: "Print",
			width: "100",
			height: "30",
			click: $scope.onClick11
		}
    }];

    $scope.onClickReqHR = function (args) {

        var gridObj = $("#GridReqHR").data("ejGrid");
        //getting corresponding record             
        var data = gridObj.getSelectedRecords()[0];
        //alert('jj' + data.Id);
        // $scope.valuePassInDelModal(data); 
        location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.Id;

    };
    $scope.commandReqHRPrint = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "100",
            height: "30",
            click: $scope.onClickReqHR
        }
    }];

	$scope.onClick111 = function (z) {

        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
		//getting corresponding record             
		var data = gridObj.getSelectedRecords()[0];
		//alert('jj' + data.Id);
		// $scope.valuePassInDelModal(data); 
		location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.Id;

	};
	$scope.command11 = [{
		type: "details", buttonOptions: {
			text: "Print",
			width: "100",
			height: "30",
			click: $scope.onClick111
		}
	}];

    //#endregion

	$window.onresize = function (event) {

		$scope.actionCompleteSelected1();

	};
	$scope.actionCompleteSelected1 = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#GridPO1").ejGrid("instance");
				var scrollerwidth = $("#checked").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};
	$window.onresize = function (event) {

		$scope.actionCompleteSelected();

	};
	$scope.actionCompleteSelected = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#GridPO").ejGrid("instance");
				var scrollerwidth = $("#id1").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
    };

    $window.onresize = function (event) {

        $scope.ReqistionHRScroll();

    };
    $scope.ReqistionHRScroll = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridReqHR").ejGrid("instance");
                var scrollerwidth = $("#id2").width();//Obtain the width of the container

                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };

      //#region Requisition Scroll 
	$window.onresize = function (event) {

		$scope.actionCompleteSelected2();

	};
	$scope.actionCompleteSelected2 = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#GridPOAPp").ejGrid("instance");
				var scrollerwidth = $("#Unapproved").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};

    $window.onresize = function (event) {

        $scope.ApprovedReqHRScroll();

    };
    $scope.ApprovedReqHRScroll = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridReqAHR").ejGrid("instance");
                var scrollerwidth = $("#ApReqHoldReject").width();//Obtain the width of the container

                //   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };

    $window.onresize = function (event) {

		$scope.actionCompleteSelected3();

	};
	$scope.actionCompleteSelected3 = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#GridPO12").ejGrid("instance");
				var scrollerwidth = $("#approved").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
    };

     //#endregion

    //#region Requisition Print 

    $scope.onClick1 = function (args) {

        var gridObj = $("#GridPO12").data("ejGrid");
        //getting corresponding record             
        var data = gridObj.getSelectedRecords()[0];
        //alert('jj' + data.Id);
        // $scope.valuePassInDelModal(data); 
        location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.Id;

    };
    $scope.command = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "100",
            height: "30",
            click: $scope.onClick1
        }
    }];

   // #endregion    $scope.MaterialLastPOPrice = function (x) {
        $scope.GetLastPurchaseQtyGrid(x);      
    };


    $scope.MaterialLastPOPriceHide = function () {
        //$scope.taxCategoryList = [];
        angular.element(document.querySelector('#ListMaterialLastPOPrice')).modal('hide');
    };
    
    $scope.GetLastPurchaseQtyList = [];
    $scope.GetLastPurchaseQtyGrid = function (x) {

        try {            $http({                method: 'POST',                url: 'Products/InventoryCheckApproved/GetMaterialLastPOQty',                data: { 'materialMasterId': x.MaterialMasterId, 'Id': x.ArticleId, 'Sku1': x.FirstCharacteristicsId, 'Sku2': x.SecondCharacteristicsId, 'Sku3': x.ThirdCharacteristicsId },                dataType: 'JSON'            }).then(function successCallback(response) {                if (response.data.Error == true) {                    ShowResult(response.data.Message, 'failure');                }                else {                    $scope.GetLastPurchaseQtyList = response.data;
                    var eDialog = $("#dialogListMaterialLastPOPrice").data("ejDialog");
                    eDialog.open();                }            }, function errorCallback(response) {                ShowResult(response.status.Message, 'failure');            });        } catch (e) {            ShowResult(e, 'failure');        }

    };
    
} 
    
