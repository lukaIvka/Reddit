
$(document).on('click', '.trash', function () {
    var $themeContainer = $(this).closest('.theme-container');
    var themeId = $themeContainer.attr('data-rowKey');
    var token = localStorage.getItem('token');
    console.log(themeId);

    // Pozivamo akciju brisanja teme na serveru
    $.ajax({
        url: '/Theme/DeleteTheme',
        type: 'POST',
        dataType: 'json',
        data: { themeId: themeId, token: token}, // Prosleđujemo ID teme koju želimo da obrišemo
        success: function (response) {
            if (response.success) {
                $themeContainer.remove();
                alert("Topic is successfully deleted!");
            } else {
                console.error(response.message); // Ako dođe do greške, ispišemo poruku u konzoli
            }
        },
        error: function (xhr, status, error) {
            console.error(error); // Ukoliko dođe do greške u AJAX pozivu, ispišemo je u konzoli
        }
    });
});

$(document).on('click', '.subscribeButton', function () {
    var $themeContainer = $(this).closest('.theme-container');
    var themeId = $themeContainer.attr('data-rowKey');
    var token = localStorage.getItem('token');

    $.ajax({
        url: '/Theme/AddSubscriber',
        type: 'POST',
        dataType: 'json',
        data: { themeId: themeId, token: token }, 
        success: function (response) {
            if (response.success) {
                UpdateSubscribeButton(true, themeId);
                console.log('Korisnik je uspešno dodat među subscribe-ovane korisnike za temu.');
            } else {
                console.error(response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error(error); 
        }
    });
});


$(document).on('click', '.unsubscribeButton', function () {
    var $themeContainer = $(this).closest('.theme-container');
    var themeId = $themeContainer.attr('data-rowKey');
    var token = localStorage.getItem('token');

    $.ajax({
        url: '/Theme/DeleteSubscriber',
        type: 'POST',
        dataType: 'json',
        data: { themeId: themeId, token: token },
        success: function (response) {
            if (response.success) {
                // Pretplata je uspešno otkazana
                UpdateSubscribeButton(false, themeId);
                console.log('Pretplata je uspešno otkazana.');
                // Možete ažurirati UI ili obavestiti korisnika
            } else {
                console.error(response.message); // Greška prilikom otkazivanja pretplate
            }
        },
        error: function (xhr, status, error) {
            console.error(error); // Greška u AJAX pozivu
        }
    });
});


function UpdateSubscribeButton(subscribed, themeId) {
    var $button = $('[data-rowKey="' + themeId + '"] button');

    if (subscribed) {
        console.log("subscribed");
        $button.text('Unsubscribe');
        $button.removeClass('subscribeButton').addClass('unsubscribeButton');
    } else {
        console.log("unsubscribed");
        $button.text('Subscribe');
        $button.removeClass('unsubscribeButton').addClass('subscribeButton');
    }
}