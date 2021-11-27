$(function ($) {
    $.validator.addMethod('date',
    function (value, element) {
        if (this.optional(element)) {
            return true;
        }
        var ok = true;
        try {
            ParseDate(value);
        }
        catch (err) {
            ok = false;
        }
        return ok;
    });
});