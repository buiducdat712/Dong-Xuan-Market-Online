$(document).ready(function () {
    // Lấy danh sách voucher từ server khi modal được mở
    $('#voucherModal').on('show.bs.modal', function (e) {
        $.ajax({
            url: '/Cart/GetAvailableVouchers',
            type: 'GET',
            success: function (data) {
                var voucherList = '';
                data.forEach(function (voucher) {
                    voucherList += `
                        <div class="voucher-item">
                            <h6>${voucher.code}</h6>
                            <p>${voucher.description}</p>
                            <button class="btn btn-sm btn-primary apply-voucher" data-voucher-code="${voucher.code}">Áp dụng</button>
                        </div>
                    `;
                });
                $('#voucherModal .modal-body').html(voucherList);
            },
            error: function () {
                $('#voucherModal .modal-body').html('<p>Không thể tải danh sách voucher. Vui lòng thử lại sau.</p>');
            }
        });
    });

    // Xử lý khi người dùng chọn voucher
    $(document).on('click', '.apply-voucher', function () {
        var voucherCode = $(this).data('voucher-code');
        $.ajax({
            url: '/Cart/ApplyVoucher',
            type: 'POST',
            data: { voucherCode: voucherCode },
            success: function (response) {
                if (response.success) {
                    alert('Áp dụng voucher thành công!');
                    location.reload(); // Tải lại trang để cập nhật thông tin giỏ hàng
                } else {
                    alert('Không thể áp dụng voucher: ' + response.message);
                }
            },
            error: function () {
                alert('Có lỗi xảy ra. Vui lòng thử lại sau.');
            }
        });
        $('#voucherModal').modal('hide');
    });
});
