'use strict';
partyMappingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$window', '$controller'];
function partyMappingController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $window, $controller) {
    $rootScope.title = "Party Mapping";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.partyMappings = [];
    $scope.partyMappingList = [];
    $scope.path = 'Parties/PartyMapping/';
    $scope.saveUrl = $scope.path + 'Create';
    $scope.updateUrl = $scope.path + 'Edit';
    $scope.deleteUrl = $scope.path + 'Delete/';
    $controller('partyBaseController', { $scope: $scope, $http: $http });

    $scope.partyMapping = {
        Id: null
        , CompanyGroupId: null
        , CompanyId: null
        , PlantId: null
        , PartyId: null
        , PartyCode: null
        , PartyName: null
        , PartyPlantId: null
        , PartyLocation: null
        , OldPartyId: null
        , PartyType: null
    };

    $scope.partyTypeList = [
        {
            'Value': 'Customer'
            , 'Text': 'Customer'
        },
        {
            'Value': 'Vendor'
            , 'Text': 'Vendor'
        }
    ];

    $scope.searchByList = [
        {
            'name': 'Party Code',
            'value': 'PartyCode'
        },
        {
            'name': 'Party Name',
            'value': 'PartyName'
        },
        {
            'name': 'Party Location',
            'value': 'PartyLocation'
        },
        {
            'name': 'Old PartyId',
            'value': 'OldPartyId'
        },
        {
            'name': 'OldParty Name',
            'value': 'OldPartyName'
        }
    ];

    baseService.init('', null, null, null, 'PartyName, OldPartyName', 'OldPartyName');
    $scope.getDataList = function () {
        $scope.partyType = $scope.partyMapping.PartyType;
        baseService.init('Parties/PartyMapping/GetList?partyType=' + $scope.partyMapping.PartyType, null, null, null, 'PartyName, OldPartyName', 'OldPartyName');
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.partyMappingList = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    $scope.closePartyPopUp = function () {
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            $scope.partyMapping.PartyId = party.Id;
            $scope.partyMapping.PartyCode = party.PartyCode;
            $scope.partyMapping.PartyName = party.PartyName;
            $scope.getPartyLocationList(party.Id);
        }
        $scope.hidePartyPopUp();
    };

    $scope.partyPlantList = [];
    $scope.getPartyLocationList = function (partyId, partyPlantId) {
        if (!baseService.isUndefinedOrNull(partyId)) {
            $http.get('Parties/party/GetPartyPlantCbo?partyId=' + partyId)
                .then(function (response) {
                    $scope.partyPlantList = response.data;
                    angular.forEach(response.data, function (item, i) {
                        if (!baseService.isUndefinedOrNull(partyPlantId) && item.Value === partyPlantId) {
                            $scope.partyMapping.PartyPlantId = item.Value;
                            $scope.partyMapping.PartyCountry = item.CountryName;
                            $scope.partyMapping.PartyState = item.StateCode + ' - ' + item.StateName;
                            $scope.partyMapping.PartyCity = item.CityName;
                            $scope.partyMapping.PartyGSTIN = item.GSTIN;
                            $scope.partyMapping.PartyAddress = item.Address1;
                        }
                        else if (item.IsDefault) {
                            $scope.partyMapping.PartyPlantId = item.Value;
                            $scope.partyMapping.PartyCountry = item.CountryName;
                            $scope.partyMapping.PartyState = item.StateCode + ' - ' + item.StateName;
                            $scope.partyMapping.PartyCity = item.CityName;
                            $scope.partyMapping.PartyGSTIN = item.GSTIN;
                            $scope.partyMapping.PartyAddress = item.Address1;
                        }
                    });
                });
        }
    };

    $scope.getPartyLocationDetail = function (id) {
        if (!baseService.isUndefinedOrNull(id)) {
            for (var i = 0; i < baseService.arrayLength($scope.partyPlantList); i++) {
                if ($scope.partyPlantList[i].Value === id) {
                    $scope.partyMapping.PartyCountry = $scope.partyPlantList[i].CountryName;
                    $scope.partyMapping.PartyState = $scope.partyPlantList[i].StateCode + ' - ' + $scope.partyPlantList[i].StateName;
                    $scope.partyMapping.PartyCity = $scope.partyPlantList[i].CityName;
                    $scope.partyMapping.PartyGSTIN = $scope.partyPlantList[i].GSTIN;
                    $scope.partyMapping.PartyAddress = $scope.partyPlantList[i].Address1;
                }
            }
        }
        else {
            $scope.partyMapping.PartyCountry = null;
            $scope.partyMapping.PartyState = null;
            $scope.partyMapping.PartyCity = null;
            $scope.partyMapping.PartyGSTIN = null;
            $scope.partyMapping.PartyAddress = null;
        }
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.partyMapping = $scope.partyMappingList[$scope.index];
        $scope.partyPlantList = [];
        $scope.getPartyLocationList($scope.partyMapping.PartyId, $scope.partyMapping.PartyPlantId);
        $scope.partyParameters.search = "%" + $scope.partyMapping.OldPartyName + "%";
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.partyMappingNewForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST'
                    , url: $scope.saveUrl
                    , data: $scope.partyMapping
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        $scope.Clear();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST'
                    , url: $scope.updateUrl
                    , data: $scope.partyMapping
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    };

    //Deleting Rows from GLMappingList
    $scope.valuePassInDelModal = function (index, data) {
        $scope.tempPartyMappingOb = data;
        $scope.partyMappingIndex = index;
        if (baseService.isUndefinedOrNull($scope.tempPartyMappingOb.Id))
            $scope.message_confirmation = 'Are you sure want to parmenently delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + data.Party + ' ]';
        angular.element(document.querySelector('#confirmDocumentdelete')).modal('show');
    };

    $scope.removeRow = function () {
        if (baseService.isUndefinedOrNull($scope.tempPartyMappingOb.Id) === true) {
            $scope.partyMappings.splice($scope.partyMappingIndex, 1);
        } else {
            $scope.Delete($scope.tempPartyMappingOb.Id, $scope.partyMappingIndex);
        }
        $scope.partyMappingIndex = -1;
        $scope.$scope.tempPartyMappingOb.Id = null;
        angular.element(document.querySelector('#confirmDocumentdelete')).modal('hide');
    };

    $scope.Delete = function (id, index) {
        try {
            $http({
                method: 'POST',
                url: $scope.deleteUrl,
                dataType: 'JSON',
                data: { 'id': id }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.partyMappingList.splice($scope.partyMappingIndex, 1);
                    $scope.partyMappingIndex = -1;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.Clear = function () {
        $scope.partyMapping = {};
        $scope.partyPlantList = [];
    };
}