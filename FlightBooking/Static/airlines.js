$(document).ready(function () {
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

    function loadAirlines(data) {
        $('#airlinesList').empty();
        data.forEach(function (airline) {
            $('#airlinesList').append('<li class="list-group-item"><a href="airlineDetails.html?name=' + encodeURIComponent(airline.Name) + '">' + airline.Name + '</a></li>');
        });
    }

    function fetchAirlines() {
        $.ajax({
            url: '/api/airlines',
            method: 'GET',
            success: function (data) {
                loadAirlines(data);
            },
            error: function (xhr, status, error) {
                alert('Failed to load airlines: ' + xhr.responseText);
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
                fetchAirlines();
            },
            error: function (xhr, status, error) {
                alert('Failed to check authentication: ' + xhr.responseText);
                loadNavBar(false);
                fetchAirlines();
            }
        });
    }

    checkAuthentication();
});