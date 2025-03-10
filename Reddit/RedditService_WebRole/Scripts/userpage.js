function sendDataToBackend() {
    window.location.href = 'UserProfile';
}

function updateUI() {
    if (localStorage.getItem('token') == null) {
        $('.auth-container').html('<button onclick="loginNavigate()">Login</button>');
    } else {
        $('.auth-container').html('<button onclick="sendDataToBackend()">Profile</button> <button onclick="logout()">Logout</button>');
    }
}

function loginNavigate() {
    window.location.href = '../Authentication/login';
}

function logout() {
    localStorage.removeItem('token');
    updateUI();
    window.location.href = '/Authentication/Login';
}

$(document).ready(function () {
    updateUI();
});

function newTopicNavigate() {
    window.location.href = "/Theme/NewTheme";
}

function getEmailFromToken(token) {
    try {
        // Dekodiraj JWT token
        const tokenParts = token.split('.');
        const payload = JSON.parse(atob(tokenParts[1]));

        console.log(payload);

        // Pronađi email u payloadu tokena
        const email = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'];
        return email;
    } catch (error) {
        // Greška prilikom dekodiranja tokena
        //window.location.href = '../Authentication/Login';
        console.error('Error decoding token skrrrr:', error);
        return null;
    }
}
 //Upvote poziv funkcije
function handleUpvote(themeId) {
    //theme.Upvote++; 
    // $('.upvote-count').text(theme.Upvote + '  '); // Update the upvote count in the UI
    sendUpvote(themeId); // Send the updated count and token to the backend
}
//Upvote funkcija
function sendUpvote(rowKey) {
    var token = localStorage.getItem('token'); // Retrieve the token from localStorage

    $.ajax({
        url: '/User/Upvote', // Replace with your controller/action
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        data: JSON.stringify({
            token: token,
            rowKey: rowKey
        }),
        success: function (response) {
            console.log('Success:', response);
            if (response.user) {
                loadThemes();
            } else {
                window.location.href = '../Authentication/Login';
            }
            
        },
        error: function (xhr, status, error) {
            console.error('Error:', status, error);
        }
    });
}
//Downvote poziv funkcije
function handleDownvote(themeId) {
    //theme.Upvote++; 
    // $('.upvote-count').text(theme.Upvote + '  '); // Update the upvote count in the UI
    sendDownvote(themeId); // Send the updated count and token to the backend
}
//Downvote funkcija
function sendDownvote(rowKey) {
    var token = localStorage.getItem('token'); // Retrieve the token from localStorage

    $.ajax({
        url: '/User/Downvote', // Replace with your controller/action
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        data: JSON.stringify({
            token: token,
            rowKey: rowKey
        }),
        success: function (response) {
            console.log('Success:', response);
            if (response.user) {
                loadThemes();
            } else {
                console.log('Not logged in:', response);
                window.location.href = '/Authentication/Login';
            }
        },
        error: function (xhr, status, error) {
            console.error('Error:', status, error);
        }
    });
}

function loadThemes(sortOrder) {
    // Dobavi JWT token iz localStorage-a
    var token = localStorage.getItem('token');
    var userEmail = getEmailFromToken(token);
    console.log(userEmail);

    // Isprazni listu tema pre nego što ponovo učitaš teme
    $('.topics').empty();
    // Učitaj teme korisnika koji je ulogovan
    $.ajax({
        url: '/Theme/LoadThemes',
        type: 'GET',
        dataType: 'json',
        data: {
            sortOrder: sortOrder // Prosledi parametar za sortiranje na server
        },
        success: function (response) {
            if (response.success) {
                var userThemes = response.themes.filter(function (theme) {
                    return theme.Publisher === userEmail; // Isključi teme korisnika koji je ulogovan
                });
                getAllSubscriptions(function (subscriptions) {
                    displayThemes(userThemes, 'Your topics', true, subscriptions);
                });
            } else {
                console.error(response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });

    // Učitaj sve ostale teme
    $.ajax({
        url: '/Theme/LoadThemes',
        type: 'GET',
        dataType: 'json',
        data: {
            sortOrder: sortOrder // Prosledi parametar za sortiranje na server
        },
        success: function (response) {
            if (response.success) {
                var otherThemes = response.themes.filter(function (theme) {
                    return theme.Publisher !== userEmail; // Isključi teme korisnika koji je ulogovan
                });
                getAllSubscriptions(function (subscriptions) {
                    displayThemes(otherThemes, 'Other topics', false, subscriptions);
                });
            } else {
                console.error(response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}


function getAllSubscriptions(callback) {
    $.ajax({
        url: '/Theme/GetAllSubscriptions',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            if (response.success) {
                var subscriptions = response.subs;

                console.log(subscriptions);
                callback(subscriptions);
            } else {
                console.error(response.message); // Ako dođe do greške, ispišemo poruku u konzoli
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function searchThemes() {
    var searchText = document.getElementById('searchInput').value.toLowerCase();
    var $themes = $('.theme-container');

    $themes.each(function () {
        var $theme = $(this);
        var title = $theme.find('h2').text().toLowerCase();

        if (title.includes(searchText)) {
            $theme.show();
        } else {
            $theme.hide();
        }
    });
}

function sortThemes() {
    var sortOrder = document.getElementById('sortOrder').value;
    loadThemes(sortOrder);
}

function displayThemes(themes, title, loggedInUser, subscriptions) {
    var $topicsList = $('.topics');
    $topicsList.append('<h3>' + title + '</h3><hr/><br/>');

    var token = localStorage.getItem('token');
    var userEmail = getEmailFromToken(token);

    themes.forEach(function (theme) {
       
        var $themeContainer = $('<div class="theme-container"></div>');
        $themeContainer.attr('data-rowKey', theme.RowKey);


        if (loggedInUser) {
            var $trash = $('<span class="trash">🗑️</span>');
            $themeContainer.append($trash);
        }
        else {
            // Provera da li je korisnik pretplaćen na temu
            var isSubscribed = false;

            if (subscriptions && Array.isArray(subscriptions)) {
                subscriptions.forEach(function (subscription) {
                    if (subscription.ThemeId === theme.RowKey && subscription.UserId === userEmail) {
                        isSubscribed = true;
                        return; // Prekida petlju kada se nađe odgovarajuća pretplata
                    }
                });
            } else {
                console.error("Subscriptions nisu definisane ili nisu niz.");
            }



            if (isSubscribed) {
                var $unsubscribeButton = $('<button class="unsubscribeButton">Unsubscribe</button>');
                $themeContainer.append($unsubscribeButton);
            } else {
                var $subscribeButton = $('<button class="subscribeButton">Subscribe</button>');
                $themeContainer.append($subscribeButton);
            }
        }
        var $title = $('<h2></h2>').append(
            $('<a></a>')
                .attr('href', '/User/ThemeDetails?id=' + encodeURIComponent(theme.RowKey))
                .text(theme.Title)
        );
        $themeContainer.append($title);

        if (theme.Publisher) {
            var $publisher = $('<h5></h5>').text(theme.Publisher);
            $themeContainer.append($publisher)
        }
       
        var $content = $('<p></p>').text(theme.Content);
        $themeContainer.append($content);


        if (theme.PhotoUrl) {
            var $image = $('<img>').attr('src', theme.PhotoUrl).attr('alt', 'Theme Image');
            $themeContainer.append($image);
        }

        var $votes = $('<div class="votes"></div>');

        
        $votes.append('<i class="fas fa-arrow-up" onclick="handleUpvote(\'' + theme.RowKey + '\')"></i>');
        $votes.append('<span class="upvote-count">' + theme.Upvote + '  </span>');

        $votes.append('<i class="fas fa-arrow-down" onclick="handleDownvote(\'' + theme.RowKey + '\')"></i>');
        $votes.append('<span class="downvote-count">' + theme.Downvote + '</span>');

        
        $themeContainer.append($votes);


        $topicsList.append($themeContainer);
    });
}

loadThemes();
