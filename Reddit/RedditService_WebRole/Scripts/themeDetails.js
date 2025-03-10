// themeDetails.js

$(document).ready(function () {
    loadThemeDetails();
    loadComments();
    console.log("RADI LI OVOOOO");
    /*
    $('#submit-comment').click(function () {
        submitComment();
    });*/
});

function goBackToHome() {
    window.location.href = '/User/UserPage';
}

function getThemeId() {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get('id');
}

function loadThemeDetails() {
    const token = localStorage.getItem('token');
    const themeId = getThemeId();

    $.ajax({
        url: '/Theme/GetThemeDetails',
        type: 'GET',
        data: { id: themeId },
        dataType: 'json',
        success: function (response) {
            if (response.success) {
                $('#theme-title').text(response.theme.Title);
                $('#theme-content').text(response.theme.Content);
                $('#theme-image').attr('src', response.theme.PhotoUrl);
                $('#theme-upvotes').text(response.theme.Upvote);
                $('#theme-downvotes').text(response.theme.Downvote);
                $('#theme-publisher').text(response.theme.Publisher);
                console.log('Raw Time Published:', response.theme.Time_published);

                const timestampMatch = response.theme.Time_published.match(/\/Date\((\d+)\)\//);
                if (timestampMatch) {
                    const timestamp = parseInt(timestampMatch[1], 10);
                    const publishedDate = new Date(timestamp);
                    $('#theme-time').text(publishedDate.toLocaleString());
                } else {
                    console.error('Invalid date format:', response.theme.Time_published);
                    $('#theme-time').text('Invalid date');
                }


                $('#theme-publisher').text(response.theme.Publisher);
            } else {
                console.error(response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function loadComments() {
    const token = localStorage.getItem('token');
    const themeId = getThemeId();

    $.ajax({
        url: '/Theme/GetComments',
        type: 'POST',
        data: { id: themeId },
        dataType: 'json',
        success: function (response) {
            if (response.success) {
                console.log("Comments:", response.comments);
                const commentsList = $('#comments-list');
                commentsList.empty();
                response.comments.forEach(function (comment) {
                    console.log("Comment:", comment.RowKey);
                    const commentText = $('<span></span>').html(`${comment.Content}  <b>Published by:</b> ${comment.Publisher}`);
                    const voteContainer = $('<div></div>').addClass('vote-container');
                    const upvoteButton = $('<button></button>')
                        .addClass('upvote-button')
                        .text('⬆️').click(function () {
                            handleCommentUpvote(comment.RowKey);
                        });
                    const downvoteButton = $('<button></button>')
                        .addClass('downvote-button')
                        .text('⬇️').click(function () {
                        handleCommentDownvote(comment.RowKey);
                    });
                    const upvoteCount = $('<span class="upvote-count"></span>').addClass('upvote-count').text(` ${comment.Upvote}`);
                    const downvoteCount = $('<span class="downvote-count"></span>').addClass('downvote-count').text(` ${comment.Downvote}`);

                    // Creating delete button
                    const deleteButton = $('<button></button>').text('🗑️').click(createDeleteHandler(comment.RowKey));
                    
                    // Creating a container for comment text and delete button
                    const commentItem = $('<li></li>').addClass('comment-item');
                    const commentContainer = $('<div></div>').addClass('comment-container');
                    commentContainer.append(commentText);
                    voteContainer.append(upvoteButton, upvoteCount, downvoteButton, downvoteCount);
                    commentContainer.append(voteContainer);
                    commentContainer.append(deleteButton);
                    commentItem.append(commentContainer);
                    commentsList.append(commentItem);
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


function handleCommentUpvote(commentId) {
    // Implement the logic for handling upvote for a comment
    console.log('Upvoted comment with ID:', commentId);
    sendUpvote(commentId);
}

function handleCommentDownvote(commentId) {
    // Implement the logic for handling downvote for a comment
    console.log('Downvoted comment with ID:', commentId);
    sendDownvote(commentId)
}

function sendUpvote(rowKey) {
    var token = localStorage.getItem('token'); // Retrieve the token from localStorage

    $.ajax({
        url: '/Theme/UpvoteComment', // Replace with your controller/action
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
                loadComments();
            } else {
                window.location.href = '../Authentication/Login';
            }

        },
        error: function (xhr, status, error) {
            console.error('Error:', status, error);
        }
    });
}

function sendDownvote(rowKey) {
    var token = localStorage.getItem('token'); // Retrieve the token from localStorage

    $.ajax({
        url: '/Theme/DownvoteComment', // Replace with your controller/action
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
                loadComments();
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


function deleteComment(commentId) {
    const token = localStorage.getItem('token');

    const data = {
        token: token,
        commentId: commentId
    };

    $.ajax({
        url: '/Theme/DeleteComment',
        type: 'POST',
        data: data,
        dataType: 'json',
        success: function (response) {
            if (response.success) {
                console.log("vracen success");
                loadComments();
            } else {
                console.error(response.message);
                alert(response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function AddComment() {
    
    const commentContent = document.getElementById('comment-content').value;
    const token = localStorage.getItem('token');
    console.log("token: ", token);
    const themeId = getThemeId(); 

    const data = {
        token: token,
        themeId: themeId,
        content: commentContent
    };

    // Send the data to the backend using AJAX
    $.ajax({
        url: '/Theme/SubmitComment',
        type: 'POST',
        data: data,
        dataType: 'json',
        success: function (response) {
            if (response.success) {

                loadComments();
                // Clear the textarea
                document.getElementById('comment-content').value = '';
            } else {
                // Handle error
                console.error(response.message);
            }
        },
        error: function (xhr, status, error) {
            // Handle error
            console.error(error);
        }
    });
}

function createDeleteHandler(commentId) {
    return function () {
        console.log("Deleting comment with ID:", commentId);
        deleteComment(commentId);
    };
}
