baseService.$inject = ['$http', '$rootScope', '$filter', 'paginationService'];
function baseService($http, $rootScope, $filter, paginationService) {
    var service = {
        find: find
        , init: init
        , pagination: pagination
        , paginationBase: paginationBase
        , paginationAdd: paginationAdd
        , paginationRemove: paginationRemove
        , isUndefinedOrNull: isUndefinedOrNull
        , populateSearchList: populateSearchList
        , getDDLSearchColumn: getDDLSearchColumn
        , arrayLength: arrayLength
        , setCurrentPage: setCurrentPage
        , getCompanyConfiguration: getCompanyConfiguration
        , isSequenceValidInList: isSequenceValidInList
        , checkSequence: checkSequence
        , isAvailableInList: isAvailableInList
        , getColumnValueList: getColumnValueList
    };

    function getCompanyConfiguration(callback) {
        $http.get('Organizations/Company/GetCompanyConfiguration')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // Global variable
    $rootScope.OrganizationLogoOrImage = '~/Areas/Organizations/Images';
    $rootScope.EmployeeImage = '/EmpPic/';
    //$rootScope.EmployeeDocument = '/Documents/Pratibha Syntex';
    //$rootScope.EmployeeDocument = '/Documents/Jindal';
    $rootScope.paginationUrl = null;
    $rootScope.total_count = 0;
    $rootScope.pageSize = 10;
    $rootScope.parameters = {
        limit: null,
        offset: null,
        order: null,
        sort: null,
        searchBy: null,
        search: null,
        serverPagination: null
    };

    // Arrary, property value, property name
    function find(array, pval, pname) {
        //var x = pname || 'Id';
        //var xx = { x: pval };
        return $filter("filter")(array, { Id: pval })[0];
    }

    // sPagination=server side pagination true or false.
    function init(url, sPagination, pageSize, order, sort, searchBy) {
        $rootScope.paginationUrl = url;
        $rootScope.parameters.serverPagination = true;
        $rootScope.parameters.limit = pageSize || $rootScope.pageSize;
        $rootScope.pageSize = pageSize || $rootScope.pageSize;
        $rootScope.parameters.order = order || 'asc';
        $rootScope.parameters.sort = sort || 'Sequence';
        $rootScope.parameters.searchBy = searchBy || 'UserName';
        $rootScope.parameters.offset = 0;
        $rootScope.parameters.search = null;
        $rootScope.total_count = 0;
        $rootScope.searchByList = [];
        populateSearchList();
    }

    // Default pagination service.
    function pagination(pageno) {
        if (isUndefinedOrNull($rootScope.parameters.searchBy) === false &&
            isUndefinedOrNull($rootScope.parameters.search) === false &&
            undefined === pageno) {
            $rootScope.parameters.offset = 0;
            if (!isNaN($rootScope.parameters.search) && $rootScope.parameters.searchBy === 'Sequence') {
                $rootScope.parameters.search = parseFloat($rootScope.parameters.search).toFixed(2);
            }
        }
        else if (undefined !== pageno && !isUndefinedOrNull(pageno)) {
            $rootScope.parameters.offset = $rootScope.parameters.limit * (pageno - 1);
        }
        // TODO: Client side pagination have to do.
        //if ($rootScope.parameters.serverPagination == false &&
        //    $rootScope.parameters.total_count === null) {
        //}
        return $http({
            method: 'GET',
            url: $rootScope.paginationUrl,
            params: $rootScope.parameters,
            headers: { 'Content-Type': 'application/json; charset=utf-8' }
        }).then(
            function successCallback(response) {
                if (response.data.Error === true) {
                    $rootScope.total_count = 0;
                    ShowResult(response.data.Message, 'failure');
                    return response.data;
                }
                else {
                    $rootScope.total_count = response.data.total;
                    return response.data;
                }
            },
            function errorCallback(response) {
                ShowResult(response.statusText, 'failure');
            });
    }

    // All parameterized pagination
    function paginationBase(url, pageno, parameters) {
        if (isUndefinedOrNull(parameters.searchBy) === false &&
            isUndefinedOrNull(parameters.search) === false &&
            undefined === pageno) {
            parameters.offset = 0;
        }
        else if (undefined !== pageno) {
            parameters.offset = parameters.limit * (pageno - 1);
        }
        //if (!isUndefinedOrNull(instanceId) && parameters.offset === 0) {
        //    paginationService.setCurrentPage(instanceId, 1);
        //}
        // TODO: Client side pagination have to do.
        //if ($rootScope.parameters.serverPagination == false &&
        //    $rootScope.parameters.total_count === null) {
        //}
        return $http({
            method: 'GET',
            url: url,
            params: parameters,
            headers: { 'Content-Type': 'application/json; charset=utf-8' }
        }).then(function successCallback(response) {
            return response.data;
        }, function errorCallback(response) {
            ShowResult(response.statusText, 'failure');
        });
    }

    function setCurrentPage(instanceId) {
        paginationService.setCurrentPage(instanceId, 1);
    }

    function paginationAdd() {
        $rootScope.total_count++;
    }

    function paginationRemove() {
        $rootScope.total_count--;
    }

    function isUndefinedOrNull(val) {
        return angular.isUndefined(val) || val === null || val == "";
    }

    function populateSearchList() {
        $rootScope.searchByList = [
            {
                'name': 'Sequence',
                'value': 'Sequence'
            },
            {
                'name': 'Code',
                'value': 'Code'
            },
            {
                'name': 'Short Name',
                'value': 'ShortName'
            },
            {
                'name': 'Standard Name',
                'value': 'StandardName'
            },
            {
                'name': 'User Name',
                'value': 'UserName'
            }
        ];
    }

    function getDDLSearchColumn(processData, list) {
        if (processData !== null) {
            var obsx = {
                Text: null,
                Value: null,
                IsVisible: null
            };
            var ob = processData[0];
            for (var i in ob) {
                obsx.Text = i.replace(/([A-Z])/g, ' $1').trim();
                obsx.Value = i;
                //obsx.IsVisible = (i.substring(0,1)=='_'?false:true);
                if (i.length > 2) {
                    var _indx = i.length - 2;
                    var _col = i.substr(_indx).toUpperCase();
                    obsx.IsVisible = _col === 'ID' ? false : true;
                }
                else {
                    if (i.toUpperCase() === 'ID') {
                        obsx.IsVisible = true;
                    }
                    else {
                        obsx.IsVisible = false;
                    }
                }
                // new addition
                if (i.toUpperCase() === "ARCHIVE") {
                    obsx.IsVisible = false;
                }
                if (i.toUpperCase() == "ID") {
                    obsx.IsVisible = false;
                }
                if (obsx.IsVisible) {
                    list.push(obsx);
                }
                obsx = {
                    Text: null,
                    Value: null,
                    IsVisible: null
                };
            }
        }
    }

    function arrayLength(arr) {
        if (arr === null) return 0;
        else if (arr === undefined) return 0;
        else return arr.length;
    }

    function isSequenceValidInList(list, fildName, newSeq, index) {
        try {
            if (index === -1) {
                for (var i = 0; i < list.length; i++) {
                    var seq = list[i][fildName];
                    if (list[i][fildName] == newSeq)
                        throw 'Duplicate ' + fildName + ' [' + newSeq + '] found in grid';
                }
            }
            else {
                for (var i = 0; i < list.length; i++) {
                    var seq = list[i][fildName];
                    if (list[i][fildName] == newSeq && i !== index)
                        throw 'Duplicate ' + fildName + ' [' + newSeq + '] found in grid';
                }
            }

        } catch (e) {
            throw e;
        }
    }
    function checkSequence(list, fildName, newSeq) {
        try {
            if (list.length !== 0) {
                if ((parseInt(list[list.length - 1][fildName]) + 1) !== newSeq)
                    throw 'Please input ' + fildName + ' in sequentially. EX: 1,2,3..';
            }
            else {
                if (1 !== newSeq)
                    throw 'Please input ' + fildName + ' 1..!';
            }

        } catch (e) {
            throw e;
        }
    }
    function isAvailableInList(oldValue, newValue, listIndex, index) {
        var isAvailable = false;
        // Id
        if (index == -1) {
            if (oldValue == newValue) {
                isAvailable = true;
                return isAvailable;
            }
        }
        else {
            if (index != listIndex) {
                if (oldValue == newValue) {
                    isAvailable = true;
                    return isAvailable;
                }
            }
        }
        return isAvailable;
    }
    function getColumnValueList(list, fieldName) {
        var idList = [];
        if (list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                idList.push(list[i][fieldName]);
            }
        }
        return JSON.stringify(idList);
    }
    // Finally return service.
    return service;
}

errorInterceptor.$inject = ['$q', '$rootScope', '$location', '$window', '$cookies'];
function errorInterceptor($q, $rootScope, $location, $window, $cookies) {
    return {
        request: function (config) {
            return config || $q.when(config);
        },
        requestError: function (request) {
            return $q.reject(request);
        },
        response: function (response) {
            if (response.data.Error === true && response.status === 401) {
                $window.location = '/#/' + $cookies.get('panel')
                    + '/' + $cookies.get('authToken')
                    + '/' + $cookies.get('groupId');
            }
            if (response.data.Error === false || response.status === 200) {
                $('.alert-danger').fadeOut();
            }
            return response || $q.when(response);
        },
        responseError: function (response) {
            if (response && response.status === 401) {
                ShowResult("Session time out!", 'failure');
                if ($cookies.get('panel') === 'cpanel') {
                    $window.location = '/#/' + $cookies.get('panel');
                }
                else {
                    $window.location = '/#/' + $cookies.get('panel')
                        + '/' + $cookies.get('authToken')
                        + '/' + $cookies.get('groupId');
                }
            }
            else if (response && response.status === 403) {
                ShowResult("You don't have permission to perform this action!", 'failure');
                return;
            }
            else if (response && response.status === 404) {
                $location.path('/' + $cookies.get('panel') + '/404/' + response.statusText);
            }
            else if (response && response.status >= 500) {
                $location.path('/' + $cookies.get('panel') + '/405/' + response.statusText);
            }
            else if (response && response.status >= -1) {
                ShowResult("Session time out!", 'failure');
                if ($cookies.get('panel') === 'cpanel') {
                    $window.location = '/#/' + $cookies.get('panel');
                }
                else {
                    $window.location = '/#/' + $cookies.get('panel')
                        + '/' + $cookies.get('authToken')
                        + '/' + $cookies.get('groupId');
                }
            }
            return $q.reject(response);
        }
    };
}

fileReader.$inject = ['$q', '$log'];
function fileReader($q, $log) {
    var onLoad = function (reader, deferred, scope) {
        return function () {
            scope.$apply(function () {
                deferred.resolve(reader.result);
            });
        };
    };

    var onError = function (reader, deferred, scope) {
        return function () {
            scope.$apply(function () {
                deferred.reject(reader.result);
            });
        };
    };

    var onProgress = function (reader, scope) {
        return function (event) {
            scope.$broadcast("fileProgress",
                {
                    total: event.total,
                    loaded: event.loaded
                });
        };
    };

    var getReader = function (deferred, scope) {
        var reader = new FileReader();
        reader.onload = onLoad(reader, deferred, scope);
        reader.onerror = onError(reader, deferred, scope);
        reader.onprogress = onProgress(reader, scope);
        return reader;
    };

    var readAsDataURL = function (file, scope) {
        var deferred = $q.defer();

        var reader = getReader(deferred, scope);
        reader.readAsDataURL(file);

        return deferred.promise;
    };

    return {
        readAsDataUrl: readAsDataURL
    };
}

dataShare.$inject = ['$rootScope'];
function dataShare($rootScope) {
    var service = {};
    service.data = false;
    service.sendData = function (data) {
        this.data = data;
        $rootScope.$broadcast('data_shared');
    };
    service.getData = function () {
        return this.data;
    };
    return service;
}