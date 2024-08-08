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

    function loadUsers(users) {
        const tbody = $('#usersTable tbody');
        tbody.empty();
        users.forEach(user => {
            tbody.append(`
                <tr>
                    <td>${user.Username}</td>
                    <td>${user.FirstName}</td>
                    <td>${user.LastName}</td>
                    <td>${user.Email}</td>
                    <td>${new Date(user.DateOfBirth).toLocaleDateString()}</td>
                    <td>${user.Gender}</td>
                    <td>${user.UserType}</td>
                </tr>
            `);
        });
    }

    function fetchUsers() {
        $.ajax({
            url: '/api/users/getAllUsers',
            method: 'GET',
            success: function (users) {
                loadUsers(users);
            },
            error: function (xhr, status, error) {
                alert('Failed to load users: ' + xhr.responseText);
            }
        });
    }

    $('#searchBtn').click(function () {
        const searchParams = {
            firstName: $('#searchName').val(),
            lastName: $('#searchLastName').val(),
            dateFrom: $('#searchDateFrom').val(),
            dateTo: $('#searchDateTo').val()
        };

        $.ajax({
            url: '/api/users/searchUsers',
            method: 'GET',
            data: searchParams,
            success: function (users) {
                loadUsers(users);
            },
            error: function (xhr, status, error) {
                alert('Search failed: ' + xhr.responseText);
            }
        });
    });

    $('#sortFirstNameAsc').click(function () {
        sortUsers('FirstName', true);
    });

    $('#sortFirstNameDesc').click(function () {
        sortUsers('FirstName', false);
    });

    $('#sortLastNameAsc').click(function () {
        sortUsers('LastName', true);
    });

    $('#sortLastNameDesc').click(function () {
        sortUsers('LastName', false);
    });

    $('#sortDateOfBirthAsc').click(function () {
        sortUsers('DateOfBirth', true);
    });

    $('#sortDateOfBirthDesc').click(function () {
        sortUsers('DateOfBirth', false);
    });

    function sortUsers(sortBy, ascending) {
        $.ajax({
            url: '/api/users/sortUsers',
            method: 'GET',
            data: { sortBy: sortBy, ascending: ascending },
            success: function (users) {
                loadUsers(users);
            },
            error: function (xhr, status, error) {
                alert('Sort failed: ' + xhr.responseText);
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
    fetchUsers();
});