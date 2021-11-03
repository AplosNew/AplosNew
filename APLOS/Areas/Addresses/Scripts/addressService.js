addressService.$inject = ['$http'];
function addressService($http) {
    var service = {
        getCboThana: getCboThana
        , getPoliceStationCboByDistrictChange: getPoliceStationCboByDistrictChange
        , getCboDistrict: getCboDistrict
        , getCboDistrictByState: getCboDistrictByState
        , getCboCity: getCboCity
        , getCboCityByCountry: getCboCityByCountry
        , getCboCityByDistrict: getCboCityByDistrict
        , getCboArea: getCboArea
        , getCboAreaByCity: getCboAreaByCity
        , getCboPostOffice: getCboPostOffice
        , getCboPostOfficeByDistrict: getCboPostOfficeByDistrict
        , getCboState: getCboState
        , getCboStateByCountry: getCboStateByCountry
        , getCountryCbo: getCountryCbo
        , getCountryByContinentCbo: getCountryByContinentCbo
        , getContinentCbo: getContinentCbo
    };

    function base(url, callback) {
        $http.get(url)
            .then(function successCallback(response) {
                callback(response.data);
            }, function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get Thana cbo list.
    function getCboThana(callback) {
        base('Addresses/PoliceStation/GetPoliceStationCbo', callback);
    }

    function getPoliceStationCboByDistrictChange(districtId, callback) {
        base('Addresses/PoliceStation/GetPoliceStationCboByDistrictChange?districtId=' + districtId, callback);
    }

    // Get District cbo list.
    function getCboDistrict(callback) {
        base('Addresses/District/GetDistrictCbo', callback);
    }

    function getCboDistrictByState(stateId, callback) {
        base('Addresses/District/GetDistrictCboByStateChange?stateId=' + stateId, callback);
    }

    // Get City cbo list.
    function getCboCity(callback) {
        base('Addresses/City/getcitycbo', callback);
    }

    function getCboCityByCountry(countryId, callback) {
        base('Addresses/City/GetCityByCountry?countryId=' + countryId, callback);
    }

    function getCboCityByDistrict(districtId, callback) {
        base('Addresses/City/GetCityCboListByDistrict?districtId=' + districtId, callback);
    }

    // Get Area cbo list.
    function getCboArea(callback) {
        base('addresses/area/getareacbo', callback);
    }

    function getCboAreaByCity(cityId, callback) {
        base('addresses/area/GetAreaByCity?cityId=' + cityId, callback);
    }

    // Get PostOffice cbo list.
    function getCboPostOffice(callback) {
        base('Addresses/PostOffice/GetPostOfficeCbo', callback);
    }

    function getCboPostOfficeByDistrict(districtId, callback) {
        base('Addresses/PostOffice/GetPostOfficeCboByDistrictChange?districtId=' + districtId, callback);
    }

    function getCboState(callback) {
        base('Addresses/State/GetStateCbo', callback);
    }

    function getCboStateByCountry(countryId, callback) {
        base('Addresses/State/GetStateCboByCountry?countryId=' + countryId, callback);
    }

    function getCountryCbo(callback) {
        base('addresses/country/getcountrycbo', callback);
    }

    function getCountryByContinentCbo(continentId, callback) {
        base('addresses/country/getcountrycbobycontinent?continentId=' + continentId, callback);
    }

    function getContinentCbo(callback) {
        base('addresses/continent/getcontinentcbo', callback);
    }

    return service;
}