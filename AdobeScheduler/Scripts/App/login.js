
$(function () {
    var adobeConnect = $.connection.adobeConnect;
    $(document).keypress(function (event) {
        console.log(event);
        var keycode = (event.keyCode ? event.keyCode : event.which);
        console.log(keycode);
        if (keycode === '13') {
            $('button#login').click();
        }
    });
    /*$.connection.hub.start().done(function () {
        $('button#login').click(function (e) {
            console.log("Beginning Login");
            adobeConnect.server.login($('#uname').val(), $('#pass').val()).done(function (res) {
                console.log("Results: " + res);
                if (res.Item2 != "") {
                    $('#request').html("<iframe src='" + res.Item2 + "'" + " ></iframe>");
                    setTimeout(function () {
                        $('#loginform').submit();
                    }, 100);

                } else {
                    html = "<div class='alert alert-error'><button type='button' class='close' data-dismiss='alert'>×</button><strong style='float:left;'>Error!</strong> Invalid Credentials </div>";
                    $('#error').html(html);
                }

            })
        });
    });*/
        $.connection.hub.start().done(function () {
            $('button#login').click(function (e) {
                console.log("Beginning Login");
                adobeConnect.server.login($('#uname').val(), $('#pass').val()).done(function (res) {
                    console.log("Results: " + res);
                    if (res !== "") {
                        $('#request').html("<iframe src='" + res + "'" + " ></iframe>");
                        setTimeout(function () {
                            $('#loginform').submit();
                        }, 100);

                    } else {
                        html = "<div class='alert alert-error'><button type='button' class='close' data-dismiss='alert'>×</button><strong style='float:left;'>Error!</strong> Invalid Credentials </div>";
                        $('#error').html(html);
                    }

                })
            });
        });
    
});
