$(document).ready(function () {
    function loadFlights(data) {
        $('#flightsTable tbody').empty();
        data.forEach(function (flight) {
            let reserveColumnContent = '';
            if (flight.Status.toLowerCase() === 'active') {
                reserveColumnContent = `
                    <input type="number" min="1" max="${flight.AvailableSeats}" id="passengerCount-${flight.Id}" placeholder="No. of Passengers">
                    <button onclick="reserveFlight(${flight.Id}, '${flight.Airline}', '${flight.Departure}', '${flight.Destination}', '${flight.DepartureDateTime}', '${flight.ArrivalDateTime}', ${flight.AvailableSeats}, ${flight.Price})">Reserve</button>
                `;
            } else {
                reserveColumnContent = 'Not Available';
            }

            $('#flightsTable tbody').append('<tr>' +
                '<td><a href="airlineDetails.html?name=' + encodeURIComponent(flight.Airline) + '">' + flight.Airline + '</a></td>' +
                '<td>' + flight.Departure + '</td>' +
                '<td>' + flight.Destination + '</td>' +
                '<td>' + new Date(flight.DepartureDateTime).toLocaleString() + '</td>' +
                '<td>' + new Date(flight.ArrivalDateTime).toLocaleString() + '</td>' +
                '<td>' + flight.AvailableSeats + '</td>' +
                '<td>' + flight.Price + '</td>' +
                '<td>' + flight.Status + '</td>' +
                '<td>' + reserveColumnContent + '</td>' +
                '</tr>');
        });
    }

    function fetchFlights(url, params) {
        $.ajax({
            url: url,
            method: 'GET',
            data: params,
            success: function (data) {
                loadFlights(data);
            }
        });
    }

    window.reserveFlight = function (flightId, airline, departure, destination, departureDateTime, arrivalDateTime, availableSeats, price) {
        const passengerCount = parseInt($(`#passengerCount-${flightId}`).val());
        const username = $('#usernameDisplay').text();

        if (!passengerCount || passengerCount <= 0 || passengerCount > availableSeats) {
            alert(`Please enter a valid number of passengers (1 to ${availableSeats}).`);
            return;
        }

        const newReservation = {
            Flight: {
                Id: flightId,
                Airline: airline,
                Departure: departure,
                Destination: destination,
                DepartureDateTime: departureDateTime,
                ArrivalDateTime: arrivalDateTime,
                AvailableSeats: availableSeats,
                Price: price,
                Status: 'ACTIVE'
            },
            PassengerCount: passengerCount,
            User: { Username: username },
            TotalPrice: price * passengerCount,
            Status: 'CREATED'
        };

        $.ajax({
            url: '/api/reservations/create',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(newReservation),
            success: function (response) {
                alert(response);
                // Optionally refresh the list of flights to update seat availability
                fetchFlights('/api/flights/all', { username: username });
            },
            error: function (xhr, status, error) {
                alert(xhr.responseText);
            }
        });
    }

    // Check if user is authenticated
    $.ajax({
        url: '/api/users/isAuthenticated',
        method: 'GET',
        success: function (response) {
            if (!response.isAuthenticated) {
                window.location.href = 'login.html';
            } else {
                $('#usernameDisplay').text(response.username);
                // Load initial flights
                fetchFlights('/api/flights/all', { username: response.username });
            }
        }
    });

    $('#searchForm').submit(function (event) {
        event.preventDefault();
        const departure = $('#departure').val();
        const destination = $('#destination').val();
        const departureDate = $('#departureDate').val();
        const arrivalDate = $('#arrivalDate').val();
        const airline = $('#airline').val();

        fetchFlights('/api/flights/search', {
            departure: departure,
            destination: destination,
            departureDate: departureDate,
            arrivalDate: arrivalDate,
            airline: airline
        });
    });

    $('#sortPrice').change(function () {
        const sortOrder = $(this).val();
        const rows = $('#flightsTable tbody tr').get();
        rows.sort(function (a, b) {
            const keyA = parseFloat($(a).children('td').eq(6).text());
            const keyB = parseFloat($(b).children('td').eq(6).text());
            if (sortOrder === 'asc') {
                return (keyA > keyB) ? 1 : (keyA < keyB) ? -1 : 0;
            } else {
                return (keyA < keyB) ? 1 : (keyA > keyB) ? -1 : 0;
            }
        });
        $.each(rows, function (index, row) {
            $('#flightsTable').children('tbody').append(row);
        });
    });

    $('#flightStatus').change(function () {
        const statusFilter = $(this).val();
        const rows = $('#flightsTable tbody tr').get();
        rows.forEach(row => {
            const status = $(row).children('td').eq(7).text().toLowerCase();
            if (statusFilter === "" || status === statusFilter.toLowerCase()) {
                $(row).show();
            } else {
                $(row).hide();
            }
        });
    });

    $('#logout').click(function () {
        $.ajax({
            url: '/api/users/logout',
            method: 'POST',
            success: function () {
                window.location.href = 'login.html';
            }
        });
    });
});