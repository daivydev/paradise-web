// Scripts/custom-validation.js
$(function () {
    // Áp dụng cho tất cả form có class .validated-form
    $("form.validated-form").each(function () {
        var form = $(this);

        // Reset validation mặc định (tránh dính lỗi sau khi reload)
        form.removeData("validator");
        form.removeData("unobtrusiveValidation");
        $.validator.unobtrusive.parse(form);

        // Cấu hình lại validate
        form.validate({
            onkeyup: function (element) {
                if ($(element).val() !== "") {
                    this.element(element);
                }
            },
            onfocusout: function (element) {
                if ($(element).val() !== "") {
                    this.element(element);
                }
            },
            onclick: false
        });
    });
});
