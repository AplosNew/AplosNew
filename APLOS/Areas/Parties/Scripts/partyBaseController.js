partyBaseController.$inject = ['$scope', '$http', '$filter', 'baseService'];
function partyBaseController($scope, $http, $filter, baseService) {
    $scope.partyList = [];
    $scope.partyIndex = -1;
    $scope.partySelected = null;
    $scope.partyPlantId = null;
    $scope.otherpartyPlantId = null;
    $scope.OrderSpecific = 'No';
    // $scope.partyGLType need to pass from calling controller.
    // $scope.PartyType need to pass from calling controller.
    $scope.partySearchByList = [
        {
            'name': 'Account Group',
            'value': 'PartyAccountGroupName'
        },
        {
            'name': 'Currency',
            'value': 'CurrencyCode'
        }
    ];

    if (!baseService.isUndefinedOrNull($scope.partyGLType)) {
        $scope.partySearchByList.push({
            'name': 'GL Code',
            'value': $scope.partyGLType + 'GLCode'
        });
        $scope.partySearchByList.push({
            'name': 'GL Name',
            'value': $scope.partyGLType + 'GLName'
        });
        $scope.partySearchByList.push({
            'name': 'Budget Code',
            'value': $scope.partyGLType + 'BudgetCode'
        });
        $scope.partySearchByList.push({
            'name': 'Budget Name',
            'value': $scope.partyGLType + 'BudgetName'
        });
        $scope.partySearchByList.push({
            'name': 'Activity Code',
            'value': $scope.partyGLType + 'ActivityCode'
        });
        $scope.partySearchByList.push({
            'name': 'Activity Name',
            'value': $scope.partyGLType + 'ActivityName'
        });
    }

    $scope.partyParameters = {
        limit: 10
        , offset: 0
        , order: 'ASC'
        , sort: 'UserName, PartyAccountGroupName'
        , searchBy: 'UserName'
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };



    $scope.showPartyPopUp = function () {
        baseService.setCurrentPage('partyList');
        $scope.getPartyList = function (pageno) {
            if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataList?partyType=' + $scope.partyType;
            }
            else if ($scope.partyType === 'Party') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataList';
            }
            else if ($scope.partyType === 'Director') {
                $scope.partyUrl = 'Parties/party/GetCompanyDirectorDataList';
            }
            else if ($scope.partyType === 'Other') {
                $scope.partyUrl = 'Parties/party/GetCompanyOtherDataList';
            }
            baseService.paginationBase($scope.partyUrl, pageno, $scope.partyParameters)
                .then(function (result) {
                    $scope.partyList = result.Rows;
                    $scope.partyParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#partyPopUp')).modal('show');
        $scope.getPartyList();
    };
    $scope.searchByParty = "UserName"; $scope.searchParty = "";
    $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];
    $scope.searchByParty_Loan = "UserName"; $scope.searchParty_Loan = "";
    $scope.searchByPartyList_Loan = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: "Party Name" }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];

    $scope.showPartyPopUpNew = function () {
        //if ($scope.OrderSpecific === 'Yes') {
        //	if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {

        //	}
        //	$http({
        //		method: 'POST',
        //		url: 'Parties/party/GetCompanyPartyDataListByContract?ContractId=' + $scope.productNew.ContractId + '&partyType=' + $scope.partyType,
        //		data: { column: $scope.searchByParty, value: $scope.searchParty },
        //		dataType: 'JSON'
        //	}).then(function successCallback(response) {
        //		$scope.partyList = response.data;
        //		if ($scope.partyList.length === 0) {
        //			if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
        //				$scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew?partyType=' + $scope.partyType;
        //			}
        //			else if ($scope.partyType === 'Party') {
        //				$scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
        //			}
        //			else if ($scope.partyType === 'Director') {
        //				$scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
        //			}
        //			else if ($scope.partyType === 'Other') {
        //				$scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
        //			}
        //			$http({
        //				method: 'POST',
        //				url: $scope.partyUrl,
        //				data: { column: $scope.searchByParty, value: $scope.searchParty },
        //				dataType: 'JSON'
        //			}).then(function successCallback(response) {
        //				$scope.partyList = response.data;
        //			});
        //		}
        //	});

        //}
        //else {

        if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor' || $scope.partyType === 'Director') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew?partyType=' + $scope.partyType;
        }
        else if ($scope.partyType === 'Party') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
        }
        else if ($scope.partyType === 'Director') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
        }
        else if ($scope.partyType === 'Other') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
        }
        $http({
            method: 'POST',
            url: $scope.partyUrl,
            data: { column: $scope.searchByParty, value: $scope.searchParty },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.partyList = response.data;
        });
        //}
        angular.element(document.querySelector('#partyPopUp')).modal('show');
    };

    $scope.showPartyPopUpNew_Invoice = function () {
        if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor' || $scope.partyType === 'Director') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew_Invoice?partyType=' + $scope.partyType;
        }
        $http({
            method: 'POST',
            url: $scope.partyUrl,
            data: { column: $scope.searchByParty, value: $scope.searchParty },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.partyList = response.data;
        });
        angular.element(document.querySelector('#partyPopUp_Invoice')).modal('show');
    };

    $scope.GetPartyPopUp = function () {
        if ($scope.OrderSpecific === 'Yes') {
            if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {

            }
            $http({
                method: 'POST',
                url: 'Parties/party/GetCompanyPartyDataListByContract?ContractId=' + $scope.productNew.ContractId + '&partyType=' + $scope.partyType,
                data: { column: $scope.searchByParty, value: $scope.searchParty },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.partyList = response.data;
                if ($scope.partyList.length === 0) {
                    if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
                        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew?partyType=' + $scope.partyType;
                    }
                    else if ($scope.partyType === 'Party') {
                        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
                    }
                    else if ($scope.partyType === 'Director') {
                        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
                    }
                    else if ($scope.partyType === 'Other') {
                        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
                    }
                    $http({
                        method: 'POST',
                        url: $scope.partyUrl,
                        data: { column: $scope.searchByParty, value: $scope.searchParty },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        $scope.partyList = response.data;
                    });
                }
            });

        }
        else {

            if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew?partyType=' + $scope.partyType;
            }
            else if ($scope.partyType === 'Party') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
            }
            else if ($scope.partyType === 'Director') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
            }
            else if ($scope.partyType === 'Other') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
            }
            $http({
                method: 'POST',
                url: $scope.partyUrl,
                data: { column: $scope.searchByParty, value: $scope.searchParty },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.partyList = response.data;
            });
        }
        angular.element(document.querySelector('#partyPopUp')).modal('show');
    };


    $scope.showPartyPopUpForJWPO = function (ContractId) {
        //$scope.productNew.ContractId = null;
        if ($scope.OrderSpecific === 'Yes') {
            if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {

            }
            $http({
                method: 'POST',
                url: 'Parties/party/GetCompanyPartyDataListByContract?ContractId=' + null + '&partyType=' + $scope.partyType,
                data: { column: $scope.searchByParty, value: $scope.searchParty },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.partyList = response.data;
                if ($scope.partyList.length === 0) {
                    if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
                        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew?partyType=' + $scope.partyType;
                    }
                    else if ($scope.partyType === 'Party') {
                        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
                    }
                    else if ($scope.partyType === 'Director') {
                        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
                    }
                    else if ($scope.partyType === 'Other') {
                        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
                    }
                    $http({
                        method: 'POST',
                        url: $scope.partyUrl,
                        data: { column: $scope.searchByParty, value: $scope.searchParty },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        $scope.partyList = response.data;
                    });
                }
            });

        }
        else {

            if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew?partyType=' + $scope.partyType;
            }
            else if ($scope.partyType === 'Party') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
            }
            else if ($scope.partyType === 'Director') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
            }
            else if ($scope.partyType === 'Other') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
            }
            $http({
                method: 'POST',
                url: $scope.partyUrl,
                data: { column: $scope.searchByParty, value: $scope.searchParty },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.partyList = response.data;
            });
        }
        angular.element(document.querySelector('#partyPopUp')).modal('show');
    };



    $scope.closePartyPopUpNew = function () {
        angular.element(document.querySelector('#partyPopUp')).modal('hide');
    };
    $scope.closePartyPopUpNew_Loan = function () {
        angular.element(document.querySelector('#partyPopUp_Loan')).modal('hide');
    };
    $scope.closePartyPopUpNew_Invoice = function () {
        angular.element(document.querySelector('#partyPopUp_Invoice')).modal('hide');
    };
    $scope.showNotePartyPopUpNew = function () {
        if ($scope.OrderSpecific === 'Yes') {
            if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {

            }
            $http({
                method: 'POST',
                url: 'Parties/party/GetCompanyPartyDataListByContract?ContractId=' + $scope.productNew.ContractId + '&partyType=' + $scope.partyType,
                data: { column: $scope.searchByParty, value: $scope.searchParty },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.partyList = response.data;
                if ($scope.partyList.length === 0) {
                    if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
                        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew?partyType=' + $scope.partyType;
                    }
                    else if ($scope.partyType === 'Party') {
                        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
                    }
                    else if ($scope.partyType === 'Director') {
                        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
                    }
                    else if ($scope.partyType === 'Other') {
                        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
                    }
                    $http({
                        method: 'POST',
                        url: $scope.partyUrl,
                        data: { column: $scope.searchByParty, value: $scope.searchParty },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        $scope.partyList = response.data;
                    });
                }
            });

        }
        else {

            if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew?partyType=' + $scope.partyType;
            }
            else if ($scope.partyType === 'Party') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
            }
            else if ($scope.partyType === 'Director') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
            }
            else if ($scope.partyType === 'Other') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
            }
            $http({
                method: 'POST',
                url: $scope.partyUrl,
                data: { column: $scope.searchByParty, value: $scope.searchParty },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.partyList = response.data;
            });
        }
        angular.element(document.querySelector('#notepartyPopUp')).modal('show');
    };
    $scope.showPartyPopUpNew_Loan = function () {
        if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew_Loan?partyType=' + $scope.partyType;
        }
        else if ($scope.partyType === 'Party') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew_Loan';
        }
        else if ($scope.partyType === 'Director') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew_Loan';
        }
        else if ($scope.partyType === 'Other') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew_Loan';
        }
        $http({
            method: 'POST',
            url: $scope.partyUrl,
            data: { column: $scope.searchByParty_Loan, value: $scope.searchParty_Loan },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.partyList = response.data;
        });
        angular.element(document.querySelector('#partyPopUp_Loan')).modal('show');
    };
    $scope.selectPartyPopUpRow = function (index, id) {
        $scope.partyIndex = index;
        $scope.selectedParty = id;
    };

    $scope.selectCustomerPopUp = function (index, id) {
        $scope.partyIndex = index;
        $scope.selectedCustomer = id;
    };

    $scope.hidePartyPopUp = function () {
        angular.element(document.querySelector('#partyPopUp')).modal('hide');
        $scope.partyIndex = -1;
        $scope.partySelected = null;
    };
    $scope.hidePartyPopUp_Invoice = function () {
        angular.element(document.querySelector('#partyPopUp_Invoice')).modal('hide');
        $scope.partyIndex = -1;
        $scope.partySelected = null;
    };

    $scope.closePartyPopUp = function (index, id) {
        $scope.partyIndex = index;
        $scope.selectedParty = id;
        angular.element(document.querySelector('#partyPopUp')).modal('hide');
        $scope.partyIndex = -1;
        $scope.partySelected = null;
    };
    $scope.hideNotePartyPopUp = function () {
        angular.element(document.querySelector('#notepartyPopUp')).modal('hide');
        $scope.partyIndex = -1;
        $scope.partySelected = null;
    };
    $scope.closeNotePartyPopUpNew = function (index, id) {
        $scope.partyIndex = index;
        $scope.selectedParty = id;
        angular.element(document.querySelector('#notepartyPopUp')).modal('hide');
        $scope.partyIndex = -1;
        $scope.partySelected = null;
    };

    $scope.partyPlantList = [];
    $scope.getPartyPlantList = function (partyId) {
        $scope.partyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + partyId)
            .then(function (response) {
                angular.forEach(response.data, function (item, i) {
                    $scope.partyPlantList.push(item);
                    if (item.IsDefault) {
                        $scope.partyPlantId = item.Value;
                        $scope.voucher.PartyPlantId = item.Value;
                        $scope.voucher.DeliveryPartyPlantId = item.Value;
                        $scope.billToAddress = item.Address1;
                        $scope.shipToAddress = item.Address1;
                    }
                });
            });
    };

    $scope.getCboPartyPlantList = function (partyId, callback) {
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + partyId)
            .then(function (response) {
                callback(response.data);
            });
    };

    // For FixedAssets  showFixedAssetsPopUp
    $scope.showFixedAssetsPopUp = function () {
        baseService.setCurrentPage('partyList');
        $scope.getPartyList = function (pageno) {
            if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataList?partyType=' + $scope.partyType;
            }
            else if ($scope.partyType === 'Party') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataList';
            }
            else if ($scope.partyType === 'Director') {
                $scope.partyUrl = 'Parties/party/GetCompanyDirectorDataList';
            }
            else if ($scope.partyType === 'Other') {
                $scope.partyUrl = 'Parties/party/GetCompanyOtherDataList';
            }
            else if ($scope.partyType === 'FixedAsset') {
                $scope.partyUrl = 'Parties/party/GetFixedAssetsDataList';
            }
            baseService.paginationBase($scope.partyUrl, pageno, $scope.partyParameters)
                .then(function (result) {
                    $scope.partyList = result.Rows;
                    $scope.partyParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#fixedAssetsPopUp')).modal('show');
        $scope.getPartyList();
    };


    // Used in Opening Balance
    $scope.getPartyPlantListWithCallBack = function (partyId, callback) {
        var partyPlantList = $filter('filter')($scope.partyPlantList, { PartyId: partyId }, true);
        if (!baseService.isUndefinedOrNull(partyPlantList) && partyPlantList.length > 0) {
            $scope.partyPlantId = partyId + '01';
            callback(partyPlantList);
        }
        else {
            $http.get('Parties/party/GetPartyPlantCbo?partyId=' + partyId)
                .then(function (response) {
                    callback(response.data);
                    angular.forEach(response.data, function (item, i) {
                        $scope.partyPlantList.push(item);
                        if (item.IsDefault) {
                            $scope.partyPlantId = item.Value;
                        }
                    });
                });
        }
    };

    $scope.getAllPartyPlantList = function (invoiceId) {
        $scope.partyPlantList = [];
        $http.get('Parties/party/GetAllPartyPlantCbo?invoiceId=' + invoiceId)
            .then(function (response) {
                $scope.partyPlantList = response.data;
            });
    };

    $scope.getAllPartyPlantJournalCbo = function (voucherId) {
        $scope.partyPlantList = [];
        $http.get('Parties/party/GetAllPartyPlantJournalCbo?voucherId=' + voucherId)
            .then(function (response) {
                $scope.partyPlantList = response.data;
            });
    };
    $scope.billShippAddress = function (id, flag) {
        if (!baseService.isUndefinedOrNull(id)) {
            for (var i = 0; i < baseService.arrayLength($scope.partyPlantList); i++) {
                if ($scope.partyPlantList[i].Value === id && flag === 'billTo') {
                    $scope.GSTIN = $scope.partyPlantList[i].GSTIN;
                    $scope.PartyPlantName = $scope.partyPlantList[i].PartyPlantName;
                    return $scope.billToAddress = $scope.partyPlantList[i].Address1;
                }
                else if ($scope.partyPlantList[i].Value === id && flag === 'shipTo')
                    return $scope.shipToAddress = $scope.partyPlantList[i].Address1;
            }
        }
        else {
            if (flag === 'billTo')
                return $scope.billToAddress = null;
            else if (flag === 'shipTo')
                return $scope.shipToAddress = null;
        }
    };

    $scope.showPartyPlantPopUp = function (partyPlantId) {
        $scope.getPartyLocationDetail(partyPlantId);
        angular.element(document.querySelector('#partyPlantPopUp')).modal('show');
    };

    $scope.partyPlant = {
        PartyCountry: null,
        PartyState: null,
        PartyCity: null,
        PartyGSTIN: null,
        PartyAddress: null
    };

    $scope.getPartyLocationDetail = function (id) {
        if (!baseService.isUndefinedOrNull(id)) {
            for (var i = 0; i < baseService.arrayLength($scope.partyPlantList); i++) {
                if ($scope.partyPlantList[i].Value === id) {
                    $scope.partyPlant.PartyCountry = $scope.partyPlantList[i].CountryName;
                    $scope.partyPlant.PartyState = $scope.partyPlantList[i].StateCode + ' - ' + $scope.partyPlantList[i].StateName;
                    $scope.partyPlant.PartyCity = $scope.partyPlantList[i].CityName;
                    $scope.partyPlant.PartyGSTIN = $scope.partyPlantList[i].GSTIN;
                    $scope.partyPlant.PartyAddress = $scope.partyPlantList[i].Address1;
                }
            }
        }
        else {
            $scope.partyPlant.PartyCountry = null;
            $scope.partyPlant.PartyState = null;
            $scope.partyPlant.PartyCity = null;
            $scope.partyPlant.PartyGSTIN = null;
            $scope.partyPlant.PartyAddress = null;
        }
    };

    $scope.notepartyPlantList = [];
    $scope.getNotePartyPlantList = function (partyId) {
        $scope.notepartyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + partyId)
            .then(function (response) {
                angular.forEach(response.data, function (item, i) {
                    $scope.notepartyPlantList.push(item);
                    if (item.IsDefault) {
                        $scope.otherpartyPlantId = item.Value;
                        $scope.voucher.OtherPartyPlantId = item.Value;
                        $scope.voucher.OtherDeliveryPartyPlantId = item.Value;
                        $scope.billToAddress = item.Address1;
                        $scope.shipToAddress = item.Address1;
                    }
                });
            });
    };

    $scope.notebillShippAddress = function (id, flag) {
        if (!baseService.isUndefinedOrNull(id)) {
            for (var i = 0; i < baseService.arrayLength($scope.notepartyPlantList); i++) {
                if ($scope.notepartyPlantList[i].Value === id && flag === 'billTo') {
                    $scope.NoteGSTIN = $scope.notepartyPlantList[i].GSTIN;
                    $scope.NotePartyPlantName = $scope.notepartyPlantList[i].PartyPlantName;
                    return $scope.NotebillToAddress = $scope.notepartyPlantList[i].Address1;
                }
                else if ($scope.notepartyPlantList[i].Value === id && flag === 'shipTo')
                    return $scope.NoteshipToAddress = $scope.notepartyPlantList[i].Address1;
            }
        }
        else {
            if (flag === 'billTo')
                return $scope.NotebillToAddress = null;
            else if (flag === 'shipTo')
                return $scope.NoteshipToAddress = null;
        }
    };

}