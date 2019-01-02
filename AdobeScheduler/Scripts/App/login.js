
$(function () {    
    $(document).keypress(function (event) {
        var keycode = (event.keyCode ? event.keyCode : event.which);
        if (keycode === '13') {
            $('button#login').click();
        }
    });
    
    $('button#login').click(function (e) {


        $('#loginform').submit();
        
    });       
});
