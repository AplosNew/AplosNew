makePositive.$inject = ['baseService'];
function makePositive(baseService) {
    return function (num, precision) {
        if (baseService.isUndefinedOrNull(precision)) {
            precision = 4;
        }
        return Math.abs(num).toFixed(precision);
    };
}

function sumByKey() {
    return function (data, key) {
        if (typeof (data) === 'undefined' || typeof (key) === 'undefined') {
            return 0;
        }
        var sum = 0;
        for (var i = data.length - 1; i >= 0; i--) {
            if (data[i][key] !== null && typeof (data[i][key]) !== 'undefined' && !isNaN(data[i][key])) {
                sum += parseFloat(data[i][key]);
            }
        }
        return sum.toFixed(4);
    };
}

// ex: 1.560000 to 1.56 <span>{{val | number| setDecimal:8}}</span> . For rounding <span>{{val | setDecimal:1}}</span>
setDecimal.$inject = ['$filter'];
function setDecimal($filter) {
    return function (input, places) {
        places = parseInt(places);
        if (input === null) return input;
        if (isNaN(input)) return input;
        var factor = '1' + Array(+(places > 0 && places + 1)).join('0');
        return Math.round(input * factor) / factor;
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

filterMultiple.$filter = ['$filter'];
function filterMultiple($filter) {
    return function (items, keyObj) {
        var filterObj = {
            data: items,
            filteredData: [],
            applyFilter: function (obj, key) {
                var fData = [];
                if (this.filteredData.length == 0)
                    this.filteredData = this.data;
                if (obj) {
                    var fObj = {};
                    if (!angular.isArray(obj)) {
                        fObj[key] = obj;
                        fData = fData.concat($filter('filter')(this.filteredData, fObj));
                    } else if (angular.isArray(obj)) {
                        if (obj.length > 0) {
                            for (var i = 0; i < obj.length; i++) {
                                if (angular.isDefined(obj[i])) {
                                    fObj[key] = obj[i];
                                    fData = fData.concat($filter('filter')(this.filteredData, fObj));
                                }
                            }
                        }
                    }
                    if (fData.length > 0) {
                        this.filteredData = fData;
                    }
                }
            }
        };

        if (keyObj) {
            angular.forEach(keyObj, function (obj, key) {
                filterObj.applyFilter(obj, key);
            });
        }

        return filterObj.filteredData;
    };
}

searchFilter.$filter = ['$filter'];
function searchFilter($filter) {
    return function (items, searchfilter) {
        var isSearchFilterEmpty = true;
        angular.forEach(searchfilter, function (searchstring) {
            if (searchstring !== null && searchstring !== "") {
                isSearchFilterEmpty = false;
            }
        });
        if (!isSearchFilterEmpty) {
            var result = [];
            angular.forEach(items, function (item) {
                var isFound = false;
                angular.forEach(item, function (term, key) {
                    if (term !== null && !isFound) {
                        term = term.toString();
                        term = term.toLowerCase();
                        angular.forEach(searchfilter, function (searchstring) {
                            searchstring = searchstring.toLowerCase();
                            if (searchstring !== "" && term.indexOf(searchstring) !== -1 && !isFound) {
                                result.push(item);
                                isFound = true;
                            }
                        });
                    }
                });
            });
            return result;
        } else {
            return items;
        }
    };
}

function groupBy() {
    return function (list, group_by) {
        var filtered = [];
        var prev_item = null;
        var group_changed = false;
        // this is a new field which is added to each item where we append "_CHANGED"
        // to indicate a field change in the list
        var new_field = group_by + '_CHANGED';
        // loop through each item in the list
        angular.forEach(list, function (item) {
            group_changed = false;
            // if not the first item
            if (prev_item !== null) {
                // check if the group by field changed
                if (prev_item[group_by] !== item[group_by])
                    group_changed = true;
                // otherwise we have the first item in the list which is new
            } else
                group_changed = true;

            // if the group changed, then add a new field to the item
            // to indicate this
            if (group_changed)
                item[new_field] = true;
            else
                item[new_field] = false;

            filtered.push(item);
            prev_item = item;
        });
        return filtered;
    };
}

//$scope.filteredArray =  $filter('unique')($scope.tabs,'Category');
function unique() {
    return function (items, filterOn) {

        if (filterOn === false) {
            return items;
        }

        if ((filterOn || angular.isUndefined(filterOn)) && angular.isArray(items)) {
            var hashCheck = {}, newItems = [];

            var extractValueToCompare = function (item) {
                if (angular.isObject(item) && angular.isString(filterOn)) {
                    return item[filterOn];
                } else {
                    return item;
                }
            };

            angular.forEach(items, function (item) {
                var valueToCheck, isDuplicate = false;

                for (var i = 0; i < newItems.length; i++) {
                    if (angular.equals(extractValueToCompare(newItems[i]), extractValueToCompare(item))) {
                        isDuplicate = true;
                        break;
                    }
                }
                if (!isDuplicate) {
                    newItems.push(item);
                }

            });
            items = newItems;
        }
        return items;
    };
};