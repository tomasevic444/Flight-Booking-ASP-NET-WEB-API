$(document).ready(function () {
    function loadReservations(data) {
        $('#reservationsTable tbody').empty();
        data.forEach(function (reservation) {
            var actionButtons = '';
            if (reservation.Status === 'CREATED' || reservation.Status === 'APPROVED') {
                actionButtons += '<button class="cancelBtn" data-id="' + reservation.Id + '">Cancel</button>';
            }
            if (reservation.Flight.Status === 'FINISHED') {
                actionButtons += '<button class="reviewBtn" data-id="' + reservation.Id + '">Add Review</button>';
            }
            $('#reservationsTable tbody').append('<tr>' +
                '<td>' + reservation.Flight.Airline + ' from ' + reservation.Flight.Departure + ' to ' + reservation.Flight.Destination + '</td>' +
                '<td>' + reservation.PassengerCount + '</td>' +
                '<td>' + reservation.TotalPrice + '</td>' +
                '<td>' + reservation.Status + '</td>' +
                '<td>' + actionButtons + '</td>' +
                '</tr>');
        });

        $('.cancelBtn').click(function () {
            var reservationId = $(this).data('id');
            $.ajax({
                url: '/api/reservations/cancel',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ ReservationId: reservationId }),
                success: function (response) {
                    alert(response);
                    location.reload(); // Reload the page to update the reservations list
                },
                error: function (xhr, status, error) {
                    alert('Failed to cancel reservation: ' + xhr.responseText);
                }
            });
        });

        $('.reviewBtn').click(function () {
            var reservationId = $(this).data('id');
            $('#reviewForm').show().data('reservationId', reservationId);
        });
    }

    function fetchReservations(params) {
        $.ajax({
            url: '/api/reservations',
            method: 'GET',
            data: params,
            success: function (data) {
                loadReservations(data);
            },
            error: function (xhr, status, error) {
                alert('Failed to load reservations: ' + xhr.responseText);
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
                // Load initial reservations
                fetchReservations({ username: response.username });
            }
        },
        error: function (xhr, status, error) {
            alert('Failed to authenticate: ' + xhr.responseText);
            window.location.href = 'login.html';
        }
    });

    $('#reservationStatus').change(function () {
        const status = $(this).val();
        const username = $('#usernameDisplay').text();
        fetchReservations({ username: username, status: status });
    });

    // Handle user logout
    $('#logout').click(function () {
        $.ajax({
            url: '/api/users/logout',
            method: 'POST',
            success: function () {
                window.location.href = 'index.html';
            }
        });
    });

    // Function to convert file to base64
    function getBase64(file, callback) {
        const reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onload = function () {
            callback(reader.result);
        };
        reader.onerror = function (error) {
            console.log('Error: ', error);
        };
    }

    // Handle review submission
    $('#submitReview').click(function () {
        var reservationId = $('#reviewForm').data('reservationId');
        var title = $('#reviewTitle').val();
        var content = $('#reviewContent').val();
        var imageFile = $('#reviewImage')[0].files[0];

        if (imageFile) {
            getBase64(imageFile, function (base64Image) {
                var reviewData = {
                    ReservationId: reservationId,
                    Title: title,
                    Content: content,
                    Image: base64Image
                };

                $.ajax({
                    url: '/api/reviews/create',
                    method: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify(reviewData),
                    success: function (response) {
                        alert(response);
                        $('#reviewForm').hide();
                        $('#reviewTitle').val('');
                        $('#reviewContent').val('');
                        $('#reviewImage').val(''); // Clear the file input
                        location.reload();
                    },
                    error: function (xhr, status, error) {
                        alert('Failed to submit review: ' + xhr.responseText);
                    }
                });
            });
        } else {
            var reviewData = {
                ReservationId: reservationId,
                Title: title,
                Content: content
            };

            $.ajax({
                url: '/api/reviews/create',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(reviewData),
                success: function (response) {
                    alert(response);
                    $('#reviewForm').hide();
                    $('#reviewTitle').val('');
                    $('#reviewContent').val('');
                    location.reload();
                },
                error: function (xhr, status, error) {
                    alert('Failed to submit review: ' + xhr.responseText);
                }
            });
        }
    });
});