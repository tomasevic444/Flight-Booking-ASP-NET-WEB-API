$(document).ready(function () {
    function checkAuthentication() {
        $.ajax({
            url: '/api/users/isAuthenticated',
            method: 'GET',
            success: function (response) {
                if (!response.isAuthenticated) {
                    window.location.href = 'login.html';
                } else if (response.userType !== 'Administrator') {
                    alert('You do not have permission to access this page.');
                    window.location.href = 'user.html';
                } else {
                    $('#usernameDisplay').text(response.username);
                }
            },
            error: function (xhr, status, error) {
                alert('Failed to check authentication: ' + xhr.responseText);
                window.location.href = 'login.html';
            }
        });
    }

    function loadReviews(reviews) {
        const tbody = $('#reviewsTable tbody');
        tbody.empty();
        reviews.forEach(review => {
            tbody.append(`
                <tr>
                    <td>${review.Reviewer}</td>
                    <td>${review.AirlineName}</td>
                    <td>${review.Title}</td>
                    <td>${review.Content}</td>
                    <td>${review.Status}</td>
                    <td>
                        <button class="approveBtn" data-id="${review.Id}">Approve</button>
                        <button class="rejectBtn" data-id="${review.Id}">Reject</button>
                    </td>
                </tr>
            `);
        });
    }

    function fetchReviews() {
        $.ajax({
            url: '/api/reviews/getAllReviews',
            method: 'GET',
            success: function (reviews) {
                loadReviews(reviews);
            },
            error: function (xhr, status, error) {
                alert('Failed to load reviews: ' + xhr.responseText);
            }
        });
    }

    $(document).on('click', '.approveBtn', function () {
        const reviewId = $(this).data('id');
        updateReviewStatus(reviewId, 'APPROVED');
    });

    $(document).on('click', '.rejectBtn', function () {
        const reviewId = $(this).data('id');
        updateReviewStatus(reviewId, 'REJECTED');
    });

    function updateReviewStatus(reviewId, status) {
        $.ajax({
            url: '/api/reviews/updateStatus',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ reviewId: reviewId, status: status }),
            success: function () {
                fetchReviews();
            },
            error: function (xhr, status, error) {
                alert('Failed to update review status: ' + xhr.responseText);
            }
        });
    }
    $('#logout').click(function () {
        $.ajax({
            url: '/api/users/logout',
            method: 'POST',
            success: function () {
                window.location.href = 'index.html';
            },
            error: function (xhr, status, error) {
                alert('Logout failed: ' + xhr.responseText);
            }
        });
    });

    checkAuthentication();
    fetchReviews();
});