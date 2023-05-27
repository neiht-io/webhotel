/// <reference path="../../scripts/jquery-3.6.4.min.js" />
/// <reference path="../../scripts/jquery-ui-1.13.2.min.js" />



$(document).ready(function () {
    $("#btnSave").click(function () {
        if (!$("#formBooking").valid()) {
            return;
        }
        AddBooking()

    });
});

function AddBooking() {
    var bookingViewModel = {};
    bookingViewModel.CustomerName = $("#txtName").val();
    bookingViewModel.CustomerAddress = $("#txtAddress").val();
    bookingViewModel.CustomerPhone = $("#txtPhone").val();
    bookingViewModel.BookingFrom = $("#txtBookingFrom").val();
    bookingViewModel.BookingTo = $("#txtBookingTo").val();
    bookingViewModel.RoomId = $("#txtRoomId").val();
    bookingViewModel.NoOfMember = $("#txtNoOfMember").val();
    bookingViewModel.PaymentTypeId = $("#txtPaymentType").val();

    $.ajax({
        async: true,
        type: 'POST',
        dataType: 'JSON',
        contentType: 'application/json;  charset=utf-8',
        url: '/Room_Booking/Create',
        data: JSON.stringify(bookingViewModel),
        success: function (data) {

        },
        error: function () {
            alert('Loi!!!!');
        }

        })
}


