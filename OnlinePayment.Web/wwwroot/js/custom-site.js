$(document).ready(function () {
    let applicationName = $('#applicationName').val();
    let applicationUrlPart = getApplicationUrlPart(applicationName);
    let intervalId = setInterval(fetchPaymentStatus, 3000);
    function fetchPaymentStatus() {
        let status = $('#paymentId').text();
        if (status === 'PAID' ||
            status === 'ERROR' ||
            status === 'CANCELLED' ||
            status === 'DECLINED') {
            clearInterval(intervalId);
            window.location.href = `${window.location.origin}/${applicationUrlPart}${status.toLowerCase()}`;
        }
        else {
            let baseUrl = window.location.origin;
            let session = $('#session').text()
            let endpoint = `${baseUrl}/${applicationUrlPart}api/v1/payments/session/${session}`;
            $.ajax({
                url: endpoint,
                method: 'GET',
                success: function (response) {
                    if (response && response.status) {
                        $('#paymentId').text(response.status);
                    }
                },
                error: function (xhr, status, error) {
                    console.error('Error fetching payment:', error);
                    $('#paymentId').text('Error fetching payment status');
                }
            });
        }
    }
    function getApplicationUrlPart(applicationName) {
        let applicationUrlPart = '';
        if (applicationName && applicationName.trim() !== '') {
            applicationUrlPart = `${applicationName}/`;
        }
        return applicationUrlPart;
    }
    fetchPaymentStatus();
});