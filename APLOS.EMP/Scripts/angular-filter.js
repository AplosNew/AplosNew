function sumByKey() {
    return function (data, key) {
        if (typeof (data) === 'undefined' || typeof (key) === 'undefined') {
            return 0;
        }
        var sum = 0;
        for (var i = data.length - 1; i >= 0; i--) {
            if (data[i][key] != null) {
                sum += parseFloat(data[i][key]);
            }
        }
        return sum.toFixed(10);
    };
}

dateFilter.$inject = ['$rootScope', 'baseService'];
function dateFilter($rootScope, baseService) {
    return function (val) {
        if (!baseService.isUndefinedOrNull(val)) {
            var reg = /\/Date\(([0-9]*)\)\//;
            if (reg.test(val)) return new Date(parseInt(val.match(reg)[1]));
            else return new Date(val);
        }
        else return null;
        //var date = new Date(input);
        //return ($filter('dateFilter')(date, 'EEE MMM dd yyyy HH:mm:ss'));
    };
}

trustUrl.$inject = ['$sce'];
function trustUrl($sce) {
    return function (recordingUrl) {
        return $sce.trustAsHtml(recordingUrl);
    };
}
safecontent.$inject = ['$sce'];
function safecontent($sce) {
    return function (val) {
        return $sce.trustAsHtml(val);
    };
}

myDate.$inject = ['$filter'];
function myDate($filter) {
    var angularDateFilter = $filter('date');
    return function (theDate) {
        return angularDateFilter(theDate, 'dd MMMM @ HH:mm:ss');
    };
}

find.$inject = ['$filter'];
function find($filter) {
    return function (array, id) {
        var dd = $filter("filter")(array, { Id: id })[0];
        console.log(dd);
        return dd;
    };
}

dateFiltering.$inject = ['$filter'];
function dateFiltering($filter) {
    return function (input) {
        if (input === null) { return ""; }
        return $filter('date')(new Date(input), 'dd-MMM-yyyy');
    };
}