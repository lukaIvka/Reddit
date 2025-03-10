function checkToken() {
    if (localStorage.getItem('token') != null) {
        alert('vec ste ulogovani');
        window.location.href = '../User/UserPage';
    }
}

$(document).ready(function () {
    checkToken();
});

async function submitLoginForm() {
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;

    try {
        const response = await fetch('/Authentication/Authenticate', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ email, password })
        });

        if (!response.ok) {
            throw new Error('Network response was not ok');
        }

        const data = await response.json();

        if (data.success) {
            const token = response.headers.get('Authorization').split(' ')[1];
            localStorage.setItem('token', token);
            // Opciono: preusmeri korisnika na drugu stranicu
            //window.location.href = '/Home/Index';
            sendTokenToServer(token);

            window.location.href = '/Home/Index';
        } else {
            console.error('Error: ' + data.error);
        }
    } catch (error) {
        console.error('There was an error:', error);
    }
}

function sendTokenToServer(token) {
    fetch('/User/SaveToken', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ token: token })
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            console.log('Token sent to server successfully');
        })
        .catch(error => {
            console.error('Error sending token to server:', error);
        });
}
