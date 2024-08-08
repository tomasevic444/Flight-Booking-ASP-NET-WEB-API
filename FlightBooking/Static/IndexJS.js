$(document).ready(function () {
    function loadFlights(data) {
        $('#flightsTable tbody').empty();
        data.forEach(function (flight) {
            $('#flightsTable tbody').append('<tr>' +
                '<td><a href="airlineDetails.html?name=' + encodeURIComponent(flight.Airline) + '">' + flight.Airline + '</a></td>' +
                '<td>' + flight.Departure + '</td>' +
                '<td>' + flight.Destination + '</td>' +
                '<td>' + new Date(flight.DepartureDateTime).toLocaleString() + '</td>' +
                '<td>' + new Date(flight.ArrivalDateTime).toLocaleString() + '</td>' +
                '<td>' + flight.AvailableSeats + '</td>' +
                '<td>' + flight.Price + '</td>' +
                '<td>' + flight.Status + '</td>' +
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

    // Initial load of active flights
    fetchFlights('/api/flights');

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
            return sortOrder === 'asc' ? keyA - keyB : keyB - keyA;
        });
        $.each(rows, function (index, row) {
            $('#flightsTable tbody').append(row);
        });
    });
});