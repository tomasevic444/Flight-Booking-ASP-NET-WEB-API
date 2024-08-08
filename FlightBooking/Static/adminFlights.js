$(document).ready(function () {
    // Function to check if the user is authenticated
    function checkAuthentication() {
        $.ajax({
            url: '/api/users/isAuthenticated',
            method: 'GET',
            success: function (response) {
                if (!response.isAuthenticated) {
                    window.location.href = 'login.html'; // Redirect to login page if not authenticated
                } else if (response.userType !== 'Administrator') {
                    alert('You do not have permission to access this page.');
                    window.location.href = 'user.html';
                } else {
                    $('#usernameDisplay').text(response.username);
                }
            },
            error: function (xhr, status, error) {
                alert('Failed to check authentication: ' + xhr.responseText); // Alert if authentication check fails
                window.location.href = 'login.html'; // Redirect to login page on failure
            }
        });
    }

    // Function to load airlines into the select dropdown
    function loadAirlines() {
        $.ajax({
            url: '/api/airlines',
            method: 'GET',
            success: function (airlines) {
                const select = $('#airlineName');
                select.empty();
                airlines.forEach(airline => {
                    select.append(`<option value="${airline.Name}">${airline.Name}</option>`);
                });
                loadFlights(); // Load flights after airlines are loaded
            },
            error: function (xhr, status, error) {
                alert('Failed to load airlines: ' + xhr.responseText); // Alert if loading airlines fails
            }
        });
    }

    // Function to load flights into the table
    function loadFlights() {
        $.ajax({
            url: '/api/flights/non-deleted',
            method: 'GET',
            success: function (flights) {
                const tbody = $('#flightsTable tbody');
                tbody.empty();
                flights.forEach(flight => {
                    const airlineOptions = $('#airlineName').html();
                    const statusOptions = `
                        <select class="statusSelect form-control">
                            <option value="ACTIVE" ${flight.Status === 'ACTIVE' ? 'selected' : ''}>ACTIVE</option>
                            <option value="FINISHED" ${flight.Status === 'FINISHED' ? 'selected' : ''}>FINISHED</option>
                            <option value="CANCELLED" ${flight.Status === 'CANCELLED' ? 'selected' : ''}>CANCELLED</option>
                        </select>
                    `;
                    const row = `
                    <tr data-id="${flight.Id}">
                        <td><select class="airlineSelect form-control">${airlineOptions}</select></td>
                        <td contenteditable="true">${flight.Departure}</td>
                        <td contenteditable="true">${flight.Destination}</td>
                        <td contenteditable="true"><input type="datetime-local" class="form-control" value="${new Date(flight.DepartureDateTime).toISOString().slice(0, 16)}"></td>
                        <td contenteditable="true"><input type="datetime-local" class="form-control" value="${new Date(flight.ArrivalDateTime).toISOString().slice(0, 16)}"></td>
                        <td contenteditable="true">${flight.AvailableSeats}</td>
                        <td>${flight.OccupiedSeats}</td>
                        <td contenteditable="true">${flight.Price}</td>
                        <td>${statusOptions}</td>
                        <td>
                            <button class="saveBtn">Edit</button>
                            <button class="deleteBtn" data-id="${flight.Id}">Delete</button>
                        </td>
                    </tr>
                `;
                    tbody.append(row);
                    // Set selected airline
                    tbody.find(`tr[data-id="${flight.Id}"] .airlineSelect`).val(flight.Airline);
                });

                // Attach event listeners to dynamically added buttons
                attachButtonListeners();
            },
            error: function (xhr, status, error) {
                alert('Failed to load flights: ' + xhr.responseText); // Alert if loading flights fails
            }
        });
    }

    $('#addFlightForm').submit(function (event) {
        event.preventDefault();

        const departureDateTime = new Date($('#departureDateTime').val());
        const arrivalDateTime = new Date($('#arrivalDateTime').val());
        const currentDate = new Date();

        // Client-side validation
        if (departureDateTime < currentDate) {
            alert('Departure date and time cannot be before the current date and time.');
            return;
        }

        if (arrivalDateTime <= departureDateTime) {
            alert('Arrival date and time cannot be before or equal to the departure date and time.');
            return;
        }

        const newFlight = {
            Airline: $('#airlineName').val(),
            Departure: $('#departure').val(),
            Destination: $('#destination').val(),
            DepartureDateTime: $('#departureDateTime').val(),
            ArrivalDateTime: $('#arrivalDateTime').val(),
            AvailableSeats: $('#availableSeats').val(),
            OccupiedSeats: 0,
            Price: $('#price').val(),
            Status: 'ACTIVE'
        };

        $.ajax({
            url: '/api/flights/add',  // Correct endpoint for adding a new flight
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(newFlight),
            success: function (response) {
                alert('Flight added successfully!');
                loadFlights(); // Reload flights after successful addition
                $('#addFlightForm')[0].reset(); // Reset the form fields
            },
            error: function (xhr, status, error) {
                alert('Failed to add flight: ' + xhr.responseText); // Alert if adding flight fails
            }
        });
    });
    // Function to attach event listeners to dynamically added buttons
    function attachButtonListeners() {
        $('.saveBtn').off('click').on('click', function () {
            const row = $(this).closest('tr');
            const flightId = row.data('id');

            const departureDateTime = new Date(row.find('input[type="datetime-local"]').eq(0).val());
            const arrivalDateTime = new Date(row.find('input[type="datetime-local"]').eq(1).val());
            const currentDate = new Date();

            // Client-side validation
            if (departureDateTime < currentDate) {
                alert('Departure date and time cannot be before the current date and time.');
                return;
            }

            if (arrivalDateTime <= departureDateTime) {
                alert('Arrival date and time cannot be before or equal to the departure date and time.');
                return;
            }

            const updatedFlight = {
                Id: flightId,
                Airline: row.find('select.airlineSelect').val(),
                Departure: row.find('td').eq(1).text(),
                Destination: row.find('td').eq(2).text(),
                DepartureDateTime: row.find('input[type="datetime-local"]').eq(0).val(),
                ArrivalDateTime: row.find('input[type="datetime-local"]').eq(1).val(),
                AvailableSeats: row.find('td').eq(5).text(),
                Price: row.find('td').eq(7).text(),
                Status: row.find('select.statusSelect').val()
            };

            $.ajax({
                url: '/api/flights/edit',
                method: 'PUT',
                contentType: 'application/json',
                data: JSON.stringify(updatedFlight),
                success: function (response) {
                    alert('Flight updated successfully!');
                    loadFlights(); // Reload flights after successful update
                },
                error: function (xhr, status, error) {
                    alert('Failed to update flight: ' + xhr.responseText);
                    loadFlights();
                    // Alert if updating flight fails
                }
            });
        });

        $('.deleteBtn').off('click').on('click', function () {
            const flightId = $(this).data('id');

            $.ajax({
                url: '/api/flights/delete?id=' + flightId,
                method: 'DELETE',
                success: function (response) {
                    alert('Flight deleted successfully!');
                    loadFlights(); // Reload flights after successful deletion
                },
                error: function (xhr, status, error) {
                    alert('Failed to delete flight: ' + xhr.responseText); // Alert if deleting flight fails
                }
            });
        });
    }
    $('#searchFlightBtn').click(function () {
        const searchDeparture = $('#searchDeparture').val();
        const searchDestination = $('#searchDestination').val();
        const searchDate = $('#searchDate').val();

        $.ajax({
            url: '/api/flights/searchFlights',
            method: 'GET',
            data: {
                departure: searchDeparture,
                destination: searchDestination,
                departureDate: searchDate
            },
            success: function (flights) {
                const tbody = $('#flightsTable tbody');
                tbody.empty();
                flights.forEach(flight => {
                    const airlineOptions = $('#airlineName').html();
                    const statusOptions = `
                        <select class="statusSelect form-control">
                            <option value="ACTIVE" ${flight.Status === 'ACTIVE' ? 'selected' : ''}>ACTIVE</option>
                            <option value="FINISHED" ${flight.Status === 'FINISHED' ? 'selected' : ''}>FINISHED</option>
                            <option value="CANCELLED" ${flight.Status === 'CANCELLED' ? 'selected' : ''}>CANCELLED</option>
                        </select>
                    `;
                    const row = `
                    <tr data-id="${flight.Id}">
                        <td><select class="airlineSelect form-control">${airlineOptions}</select></td>
                        <td contenteditable="true">${flight.Departure}</td>
                        <td contenteditable="true">${flight.Destination}</td>
                        <td contenteditable="true"><input type="datetime-local" class="form-control" value="${new Date(flight.DepartureDateTime).toISOString().slice(0, 16)}"></td>
                        <td contenteditable="true"><input type="datetime-local" class="form-control" value="${new Date(flight.ArrivalDateTime).toISOString().slice(0, 16)}"></td>
                        <td contenteditable="true">${flight.AvailableSeats}</td>
                        <td>${flight.OccupiedSeats}</td>
                        <td contenteditable="true">${flight.Price}</td>
                        <td>${statusOptions}</td>
                        <td>
                            <button class="saveBtn">Edit</button>
                            <button class="deleteBtn" data-id="${flight.Id}">Delete</button>
                        </td>
                    </tr>
                `;
                    tbody.append(row);
                    // Set selected airline
                    tbody.find(`tr[data-id="${flight.Id}"] .airlineSelect`).val(flight.Airline);
                });

                // Attach event listeners to dynamically added buttons
                attachButtonListeners();
            },
            error: function (xhr, status, error) {
                alert('Failed to search flights: ' + xhr.responseText); // Alert if search fails
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

    // Call the function to check authentication
    checkAuthentication();

    // Call the function to load airlines
    loadAirlines();
});