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

    function loadAirlines(airlines) {
        const tbody = $('#airlinesTable tbody');
        tbody.empty();
        airlines.forEach(airline => {
            tbody.append(`
                <tr>
                    <td><input type="text" class="name" value="${airline.Name}"></td>
                    <td><input type="text" class="address" value="${airline.Address}"></td>
                    <td><input type="text" class="contact" value="${airline.ContactInfo}"></td>
                    <td>
                        <button class="editBtn" data-old-name="${airline.Name}">Edit</button>
                        <button class="deleteBtn" data-name="${airline.Name}">Delete</button>
                    </td>
                </tr>
            `);
        });

        $('.editBtn').click(function () {
            const row = $(this).closest('tr');
            const oldName = $(this).data('old-name');
            const name = row.find('.name').val();
            const address = row.find('.address').val();
            const contact = row.find('.contact').val();

            const updatedAirline = {
                Name: name,
                Address: address,
                ContactInfo: contact,
                Flights: [], // Keep this as per your requirements
                Reviews: []  // Keep this as per your requirements
            };

            const requestData = {
                UpdatedAirline: updatedAirline,
                OldName: oldName
            };

            $.ajax({
                url: '/api/airlines/edit',
                method: 'PUT',
                contentType: 'application/json',
                data: JSON.stringify(requestData),
                success: function () {
                    alert('Airline updated successfully.');
                    loadAllAirlines();
                },
                error: function (xhr, status, error) {
                    alert('Failed to update airline: ' + xhr.responseText);
                    loadAllAirlines();
                }
            });
        });

        $('.deleteBtn').click(function () {
            const name = $(this).data('name');
            $.ajax({
                url: `/api/airlines/delete?name=${name}`,
                method: 'DELETE',
                success: function () {
                    alert('Airline deleted successfully.');
                    loadAllAirlines();
                },
                error: function (xhr, status, error) {
                    alert('Failed to delete airline: ' + xhr.responseText);
                    loadAllAirlines();
                }
            });
        });
    }

    function loadAllAirlines() {
        $.ajax({
            url: '/api/airlines',
            method: 'GET',
            success: function (airlines) {
                loadAirlines(airlines);
            },
            error: function (xhr, status, error) {
                alert('Failed to load airlines: ' + xhr.responseText);
            }
        });
    }

    $('#addAirlineForm').submit(function (event) {
        event.preventDefault();
        const newAirline = {
            Name: $('#airlineName').val(),
            Address: $('#airlineAddress').val(),
            ContactInfo: $('#airlineContact').val()
        };

        $.ajax({
            url: '/api/airlines/add',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(newAirline),
            success: function () {
                alert('Airline added successfully');
                loadAllAirlines();
                $('#addAirlineForm')[0].reset();
            },
            error: function (xhr, status, error) {
                alert('Failed to add airline: ' + xhr.responseText);
            }
        });
    });


    $('#searchAirlineBtn').click(function () {
        const searchParams = {
            name: $('#searchAirlineName').val(),
            address: $('#searchAirlineAddress').val(),
            contact: $('#searchAirlineContact').val()
        };

        $.ajax({
            url: '/api/airlines/search',
            method: 'GET',
            data: searchParams,
            success: function (airlines) {
                loadAirlines(airlines);
            },
            error: function (xhr, status, error) {
                alert('Search failed: ' + xhr.responseText);
            }
        });
    });


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
    loadAllAirlines();
});