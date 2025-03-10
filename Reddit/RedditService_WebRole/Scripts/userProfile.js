document.addEventListener("DOMContentLoaded", function () {
    function fetchUserData() {
        const token = localStorage.getItem('token');

        if (!token) {
            console.error('Token not found!');
            return;
        }

        fetch('/User/ProfilePage', {
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
                return response.json();
            })
            .then(data => {
                // Ažuriranje UI sa podacima o korisniku
                document.querySelector('img.profile-photo').src = data.PhotoUrl;
                document.querySelector('h1').textContent = data.Ime + ' ' + data.Prezime;
                document.querySelector('p.email').textContent = 'Email: ' + data.Email;
                document.querySelector('p.address').textContent = 'Address: ' + data.Adresa + ', ' + data.Grad + ', ' + data.Drzava;
                document.querySelector('p.phone').textContent = 'Phone: ' + data.Broj_telefona;


                // Fetch user posts and comments
                fetchUserPosts(token);
                fetchUserComments(token);

            })
            .catch(error => {
                console.error('Error fetching user data:', error);
            });
    }

    // Poziv funkcije za preuzimanje podataka o korisniku prilikom učitavanja stranice


function fetchUserPosts(token) {
    fetch('/User/GetThemes', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ token: token })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                const postsList = document.querySelector('.profile-activity ul.posts');
                postsList.innerHTML = '';
                if (data.topics.length > 0) {
                    data.topics.forEach(post => {
                        // Create list item for each post
                        console.log(post.RowKey);
                        console.log(post);
                        const li = document.createElement('li');

                        // Create title element with a link to the theme details page
                        const titleElem = document.createElement('a');
                        const link = $('<a></a>')
                            .attr('id', 'titleElem')
                            .attr('href', 'javascript:void(0);')
                            .text(post.Title)
                            .on('click', function () {
                                handleLinkClick(token, post.RowKey);
                            });
                        $(li).append(link);
                        //titleElem.textContent = post.Title;

                        // Append title to list item
                        //li.appendChild(titleElem);

                        // Append list item to posts list
                        postsList.appendChild(li);
                    });
                } else {
                    postsList.innerHTML = '<li>No posts available.</li>';
                }
            } else {
                console.error('Error fetching posts:', data.message);
            }
        })
        .catch(error => {
            console.error('Error fetching posts:', error);
        });
}

function fetchUserComments(token) {
    fetch('/User/GetComments', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ token: token })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                const commentsList = document.querySelector('.profile-activity ul.comments');
                commentsList.innerHTML = '';
                if (data.comments.length > 0) {
                    data.comments.forEach(comment => {
                        // Create list item for each comment
                        const li = document.createElement('li');

                        // Create content element
                        const contentElem = document.createElement('p');
                        contentElem.textContent = comment.Content;

                        // Append content to list item
                        li.appendChild(contentElem);

                        // Append list item to comments list
                        commentsList.appendChild(li);
                    });
                } else {
                    commentsList.innerHTML = '<li>No comments available.</li>';
                }
            } else {
                console.error('Error fetching comments:', data.message);
            }
        })
        .catch(error => {
            console.error('Error fetching comments:', error);
        });
   }

// Fetch user data on page load
fetchUserData();
});

function sendTokenToServer() {
    const token = localStorage.getItem('token');
    fetch('/User/RedirectToEdit', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ token: token })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Redirect to the profile page
                window.location.href = `/User/UpdateUser?email=${encodeURIComponent(data.email)}`;
            } else {
                // Handle error
                alert('Failed to update user.');
            }
        })
        .catch(error => console.error('Error:', error));
}


async function handleLinkClick(token, rowKey) {
    const url = `/Theme/Details`;
    const params = {
        token: token,
        id: rowKey
    };

    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(params)
        });

        if (!response.ok) {
            throw new Error('Network response was not ok');
        }

        const data = await response.json();
        console.log(data);
        if (data.success) {
            console.log("Success");
            window.location.href = '/User/ThemeDetails?id=' + encodeURIComponent(rowKey);
        }

        if (data.redirectUrl) {
            window.location.href = data.redirectUrl;
        }
    } catch (error) {
        // Handle any errors
        console.error('Error:', error);
    }
}


