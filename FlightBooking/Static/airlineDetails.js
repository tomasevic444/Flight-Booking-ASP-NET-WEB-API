$(document).ready(function () {
    const urlParams = new URLSearchParams(window.location.search);
    const airlineName = urlParams.get('name');

    function loadNavBar(isAuthenticated, username) {
        let navContent = '';
        if (isAuthenticated) {
            navContent = `
                <span class="navbar-brand mb-0 h1">User Dashboard</span>
                <div class="collapse navbar-collapse">
                    <ul class="navbar-nav ml-auto">
                        <li class="nav-item"><span class="navbar-text mr-2">Welcome, <span id="usernameDisplay">${username}</span></span></li>
                        <li class="nav-item"><a class="nav-link" href="user.html">Home</a></li>
                        <li class="nav-item"><a class="nav-link" href="airlines.html">Airlines</a></li>
                        <li class="nav-item"><a class="nav-link" href="profile.html">Profile</a></li>
                        <li class="nav-item"><a class="nav-link" href="reservations.html">My Reservations</a></li>
                        <li class="nav-item"><a class="nav-link" href="#" id="logout">Log Out</a></li>
                    </ul>
                </div>`;
        } else {
            navContent = `
                <a class="navbar-brand" href="#">Flight Reservation System</a>
                <div class="collapse navbar-collapse">
                    <ul class="navbar-nav ml-auto">
                        <li class="nav-item"><a class="nav-link" href="index.html">Home</a></li>
                        <li class="nav-item"><a class="nav-link" href="airlines.html">Airlines</a></li>
                        <li class="nav-item"><a class="nav-link" href="register.html">Register</a></li>
                        <li class="nav-item"><a class="nav-link" href="login.html">Login</a></li>
                    </ul>
                </div>`;
        }
        $('#navbar').html(navContent);

        if (isAuthenticated) {
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
        }
    }

    function loadAirlineDetails(data) {
        const airline = data.airline;
        $('#airlineDetails').empty();
        $('#airlineDetails').append('<p>Name: ' + airline.Name + '</p>' +
            '<p>Address: ' + airline.Address + '</p>' +
            '<p>Contact: ' + airline.ContactInfo + '</p>');
    }

    function loadReviews(reviews) {
        $('#reviewsList').empty();
        reviews.forEach(function (review) {
            $('#reviewsList').append('<li class="list-group-item">' +
                '<p><strong>Reviewer:</strong> ' + review.Reviewer + '</p>' +
                '<p><strong>Title:</strong> ' + review.Title + '</p>' +
                '<p><strong>Content:</strong> ' + review.Content + '</p>' +
                (review.Image ? '<img src="' + review.Image + '" alt="Review Image" class="review-image">' : '') +
                '</li>');
        });
    }

    function fetchAirlineDetails() {
        $.ajax({
            url: '/api/airlines/details',
            method: 'GET',
            data: { name: airlineName },
            success: function (response) {
                loadAirlineDetails(response);
                loadReviews(response.reviews);
            },
            error: function (xhr, status, error) {
                console.error('Error fetching airline details:', status, error);
            }
        });
    }

    function checkAuthentication() {
        $.ajax({
            url: '/api/users/isAuthenticated',
            method: 'GET',
            success: function (response) {
                if (response.isAuthenticated) {
                    loadNavBar(true, response.username);
                } else {
                    loadNavBar(false);
                }
                fetchAirlineDetails();
            },
            error: function (xhr, status, error) {
                alert('Failed to check authentication: ' + xhr.responseText);
                loadNavBar(false);
                fetchAirlineDetails();
            }
        });
    }

    checkAuthentication();
});