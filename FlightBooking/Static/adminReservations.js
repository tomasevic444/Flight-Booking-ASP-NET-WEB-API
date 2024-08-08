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

    function loadReservations() {
        $.ajax({
            url: '/api/reservations/all',
            method: 'GET',
            success: function (reservations) {
                const tbody = $('#reservationsTable tbody');
                tbody.empty();
                reservations.forEach(reservation => {
                    let actions = '';
                    if (reservation.Status === 'CREATED') {
                        actions = `
                            <button class="approveBtn" data-id="${reservation.Id}">Approve</button>
                            <button class="cancelBtn" data-id="${reservation.Id}">Cancel</button>
                        `;
                    }

                    tbody.append(`
                        <tr>
                            <td>${reservation.User.Username}</td>
                            <td>${reservation.Flight.Airline}</td>
                            <td>${reservation.Flight.Departure}</td>
                            <td>${reservation.Flight.Destination}</td>
                            <td>${new Date(reservation.Flight.DepartureDateTime).toLocaleString()}</td>
                            <td>${new Date(reservation.Flight.ArrivalDateTime).toLocaleString()}</td>
                            <td>${reservation.PassengerCount}</td>
                            <td>${reservation.TotalPrice}</td>
                            <td>${reservation.Status}</td>
                            <td>${actions}</td>
                        </tr>
                    `);
                });
            },
            error: function (xhr, status, error) {
                alert('Failed to load reservations: ' + xhr.responseText);
            }
        });
    }

    $(document).on('click', '.approveBtn', function () {
        const reservationId = $(this).data('id');
        updateReservationStatus(reservationId, 'APPROVED');
    });

    $(document).on('click', '.cancelBtn', function () {
        const reservationId = $(this).data('id');
        updateReservationStatus(reservationId, 'CANCELLED');
    });

    function updateReservationStatus(reservationId, status) {
        $.ajax({
            url: `/api/reservations/updateStatus?id=${reservationId}&status=${status}`,
            method: 'PUT',
            success: function () {
                loadReservations();
            },
            error: function (xhr, status, error) {
                alert('Failed to update reservation status: ' + xhr.responseText);
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
    loadReservations();
});