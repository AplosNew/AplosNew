baseService.$inject = ['$http', '$rootScope', '$filter', 'paginationService'];
function baseService($http, $rootScope, $filter, paginationService) {
    var service = {
        find: find
        , init: init
        , pagination: pagination
        , paginationBase: paginationBase
        , paginationPost: paginationPost
        , paginationAdd: paginationAdd
        , paginationRemove: paginationRemove
        , isUndefinedOrNull: isUndefinedOrNull
        , isUndefinedOrNaN: isUndefinedOrNaN
        , isUndefinedOrNaNOrZero: isUndefinedOrNaNOrZero
        , populateSearchList: populateSearchList
        , getDDLSearchColumn: getDDLSearchColumn
        , arrayLength: arrayLength
        , setCurrentPage: setCurrentPage
        , getCompanyConfiguration: getCompanyConfiguration
        , getNewCompanyConfiguration: getNewCompanyConfiguration
        , isSeqValid: isSeqValid
        , isSequenceValidInList: isSequenceValidInList
        , checkSequence: checkSequence
        , isAvailableInList: isAvailableInList
        , getColumnValueList: getColumnValueList
        , pk: pk
        , guid: guid
        , valueCheckInList: valueCheckInList
        , multipleValueCheckInList: multipleValueCheckInList
        , checkDecimal: checkDecimal
        , getFileExtension: getFileExtension
        , removeErrorClasses: removeErrorClasses
        , filterUnique: filterUnique
        , getIndexOf: getIndexOf
        , getMaxNumberFromList: getMaxNumberFromList
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
    }
    function getNewCompanyConfiguration(callback) {
        $http.get('Products/InventoryIssue/GetNewCompanyConfiguration')
            .then(
                function successCallback(response) {
                    callback(response.data);
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    }
    $rootScope.closePopup = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");
        try {
            $("#" + popupName).data("ejDialog").close();
        } catch (e) {

        }
    }
    $rootScope.openPopup = function (popupName) {

        try {
            $("#" + popupName).data("ejDialog").open();
        } catch (e) {

        }
    }
    $rootScope.syncfusiongriddatabound = function (args) {
        try {
            //if (args.requestType == "refresh") {

            if (args.originalEventType == "actionBegin") {
                //var gridObj = $("#GridSelectedSalesOrder").ejGrid("instance");
                //var scrollerwidth = 500;// $("#orderModal").width();//Obtain the width of the container
                ////var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                //gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20 } });//pass the obtainer width and height to gridmodel options
                //gridObj.windowonresize();


                try {

                    var gridObj1 = $(args.target.id).data("ejGrid");

                    //gridObj.refreshContent();

                    var parent = $("#" + args.target.id).parent();
                    while (parent.width() <= 101) {
                        parent = parent.parent();
                    }
                    var scrollerwidth = parent.width();
                    var gridObj = $("#" + args.target.id).ejGrid("instance");
                    gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth, height: 0 } });//pass the obtainer width and height to gridmodel options

                    gridObj1.windowonresize();
                    //gridObj.refreshContent(true);
                } catch (e) {

                }
            }
        } catch (e) {

        }
    }
    $rootScope.openPopupAngular = function (popupName) {
        try {
            angular.element(document.querySelector("#" + popupName + "")).modal("show");
        } catch (e) {

        }

    }
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
    $rootScope.moveUp = function moveUp(index, list, sequence) {
        move(index, index - 1, list, sequence);
    };
    $rootScope.moveDown = function moveDown(index, list, sequence) {
        move(index, index + 1, list, sequence);
    };
    $rootScope.tempList = [];
    //data push in temporary list by clicking check box.
    $rootScope.pushInTempList = function (data, event, list) {
        if (event.currentTarget.checked)
            $rootScope.tempList.push(data);
        else {
            for (var a = 0; a < arrayLength($rootScope.tempList); a++) {
                if ($rootScope.tempList[a].Id === data.Id)
                    $rootScope.tempList.splice(a, 1);
            }
            for (var b = 0; b < arrayLength(list); b++) {
                if (list[b].Id === data.Id)
                    return list.splice(b, 1);
            }
        }
    };
    $rootScope.genericPushInTempList = function (data, event, list, tempField, field) {
        if (event.currentTarget.checked)
            $rootScope.tempList.push(data);
        else {
            for (var a = 0; a < arrayLength($rootScope.tempList); a++) {
                if ($rootScope.tempList[a][tempField] === data[tempField])
                    $rootScope.tempList.splice(a, 1);
            }
            for (var b = 0; b < arrayLength(list); b++) {
                if (list[b][field] === data[tempField])
                    return list.splice(b, 1);
            }
        }
    };
    function move(origin, destination, list, sequence) {
        var temp = list[destination];
        list[destination] = list[origin];
        list[origin] = temp;
        if (!isUndefinedOrNull(sequence))
            for (var i = 0; i < arrayLength(list); i++) {
                list[i][sequence] = i + 1;
            }
    }

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
                    $rootScope.total_count = response.data.Total;
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
        }).then(function (response) {
            return response.data;
        }, function (response) {
            ShowResult(response.statusText, 'failure');
        });
    }

    function paginationPost(url, data) {
        return $http({
            method: 'POST',
            url: url,
            data: data,
            headers: { 'Content-Type': 'application/json; charset=utf-8' }
        }).then(function (response) {
            return response.data;
        }, function (response) {
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
        return angular.isUndefined(val) || val === null || val === "";
    }

    function isUndefinedOrNaN(val) {
        return angular.isUndefined(val) || val === null || val === "" || parseFloat(val) === 0;
    }

    function isUndefinedOrNaNOrZero(val) {
        return angular.isUndefined(val) || val === null || val === "";
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
                'name': 'User Defined Name',
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
                if (i.toUpperCase() === "ID") {
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
    function isSeqValid(list, field1, field2, message) {
        try {
            if (list === null || list.length <= 0)
                throw 'Please insert at lest one row';
            var newList = [];
            for (var i = 0; i < list.length; i++) {
                if (isUndefinedOrNull(field1) === false && isUndefinedOrNull(field2) === false)
                    if (!list[i][field1] && !list[i][field2])
                        throw message;
                var seq = list[i].Sequence;
                if (list[i].Sequence === null)
                    throw 'Sequence can not be null';
                if (newList.indexOf(seq) === -1)
                    newList.push(seq);
                else
                    throw 'Duplicate Sequence [' + seq + '] found in grid';
            }
        } catch (e) {
            throw e;
        }
    }
    function isSequenceValidInList(list, fildName, newSeq, index) {
        try {
            if (index === -1) {
                for (var i = 0; i < list.length; i++) {
                    if (list[i][fildName] === newSeq)
                        throw 'Duplicate ' + fildName + ' [' + newSeq + '] found in grid';
                }
            }
            else {
                for (var j = 0; j < list.length; j++) {
                    if (list[j][fildName] === newSeq && j !== index)
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
                if (parseInt(list[list.length - 1][fildName]) + 1 !== newSeq)
                    throw 'Please input ' + fildName + ' in sequentially. Ex: 1,2,3..';
            }
            else {
                if (1 !== newSeq)
                    throw 'Please input ' + fildName + ' 1.';
            }
        } catch (e) {
            throw e;
        }
    }
    function isAvailableInList(oldValue, newValue, listIndex, index) {
        // Id
        if (index === -1) {
            if (oldValue === newValue)
                return true;
        }
        else {
            if (index !== listIndex) {
                if (oldValue === newValue)
                    return true;
            }
        }
        return false;
    }
    function valueCheckInList(list, fildName, value) {
        for (var i = 0; i < arrayLength(list); i++) {
            if (list[i][fildName] === value) return true;
        }
        return false;
    }
    function multipleValueCheckInList(list, field1, value1, field2, value2) {
        for (var t = 0; t < arrayLength(list); t++) {
            if (list[t][field1] === value1 && list[t][field2] === value2)
                return true;
        }
        return false;
    }
    function getColumnValueList(list, fieldName) {
        var idList = [];
        for (var i = 0; i < arrayLength(list); i++) {
            idList.push(list[i][fieldName]);
        }
        return JSON.stringify(idList);
    }

    function pk() {
        return 'n-' + Math.floor(Math.random() * 900000) + 100000;
    }
    function guid() {
        function s4() {
            return Math.floor((1 + Math.random()) * 0x10000)
                .toString(16)
                .substring(1);
        }
        return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
            s4() + '-' + s4() + s4() + s4();
    };
    function checkDecimal(inputtxt) {
        var decimal = /^[-+]?[0-9]+\.[0-9]+$/;
        if (inputtxt.match(decimal)) return true;
        else if (Number.isInteger(parseInt(inputtxt))) return true;
        else return false;
    }
    function getFileExtension(filename) {
        return '.' + filename.slice((filename.lastIndexOf(".") - 1 >>> 0) + 2);
    }

    function removeErrorClasses() {
        var els = document.getElementsByClassName('has-error');
        //var els2 = document.getElementsByClassName('help-block');
        for (var i = 0; i < els.length; i++) {
            els[i].classList.remove('has-error');
        }
        //for (var t = 0; t < els2.length; t++) {
        //    els2[t].classList.remove('help-block');
        //}
    }

    function getIndexOf(arr, val, prop) {
        var l = arr.length,
            k = 0;
        for (k = 0; k < l; k = k + 1) {
            if (arr[k][prop] === val) {
                return k;
            }
        }
        return false;
    }
    function getMaxNumberFromList(list, fieldName) {
        var max = 0;
        for (var t = 0; t < arrayLength(list); t++) {
            if (list[t][fieldName] > max)
                max = parseFloat(list[t][fieldName]);
        }
        return max + 1;
    }
    // Finally return service.
    return service;
}

errorInterceptor.$inject = ['$q', '$rootScope', '$location', '$window', '$cookies'];
function errorInterceptor($q, $rootScope, $location, $window, $cookies) {
    return {
        request: function (config) {
            //if (!navigator.onLine)
            //    return ShowResult("No internet connection", 'failure');
            //btnDisabled();
            return config || $q.when(config);
        },
        requestError: function (request) {
            //btnEnabled();
            return $q.reject(request);
        },
        response: function (response) {
            if (response.data.Error === true && response.status === 401) {
                $window.location = '/#/' + $cookies.get('panel')
                    + '/' + $cookies.get('authToken')
                    + '/' + $cookies.get('groupId');
            }
            if (response.status === 200) {
                $('.alert-danger').fadeOut();
            }
            //btnEnabled();
            return response || $q.when(response);
        },
        responseError: function (response) {
            if (response && response.status === 401) {
                ShowResult("Session time out!", 'failure');
                if ($cookies.get('panel') === 'cpanel') {
                    $window.location = '/#/' + $cookies.get('panel');
                }
                else {
                    $window.location = '/' + $cookies.get('panel')
                        + '?authToken=' + $cookies.get('authToken')
                        + '&groupId=' + $cookies.get('groupId');
                }
            }
            else if (response && response.status === 403) {
                ShowResult("Session was forcibly closed by the server. Please refresh the application [Session Closed]", 'failure');
                return;
            }
            else if (response && response.status === 406) {
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
                    $window.location = '/' + $cookies.get('panel')
                        + '?authToken=' + $cookies.get('authToken')
                        + '&groupId=' + $cookies.get('groupId');
                }
            }
            return $q.reject(response);
        }
    };
}

fileReader.$inject = ['$q'];
function fileReader($q) {
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

exportToExcel.$inject = ['$window'];
function exportToExcel($window) {
    var uri = 'data:application/vnd.ms-excel;base64,',
        template = '<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40"><head><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>{worksheet}</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--></head><body><table>{table}</table></body></html>',
        base64 = function (s) { return $window.btoa(unescape(encodeURIComponent(s))); },
        format = function (s, c) { return s.replace(/{(\w+)}/g, function (m, p) { return c[p]; }); };
    return {
        tableToExcel: function (tableId, worksheetName) {
            var table = $(tableId),
                ctx = { worksheet: worksheetName, table: table.html() },
                href = uri + base64(format(template, ctx));
            return href;
        }
    };
}
function filterUnique(array, uniqueField) {
    var seen = {};
    return array.filter(function (x) {
        if (seen[x[uniqueField]])
            return;
        seen[x[uniqueField]] = true;
        return x;
    });
}