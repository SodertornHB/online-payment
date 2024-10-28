$(document).ready(function () {
    let intervalId = setInterval(fetchPaymentStatus, 3000);
    function fetchPaymentStatus() {
        let status = $('#paymentId').text();
        if (status === 'PAID') {
            clearInterval(intervalId);
        }
        else if (status === 'ERROR') {
            clearInterval(intervalId);
        }
        else if (status === 'CANCELLED') {
            clearInterval(intervalId);
        }
        else if (status === 'DECLINED') {
            clearInterval(intervalId);
        }
        else {
            let baseUrl = window.location.origin;
            let session = $('#session').text()
            let applicationName = $('#applicationName').val();
            let applicationUrlPart = getApplicationUrlPart(applicationName);
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

        function getApplicationUrlPart(applicationName) {
            let applicationUrlPart = '';
            if (applicationName && applicationName.trim() !== '') {
                applicationUrlPart = `${applicationName}/`;
            }
            return applicationUrlPart;
        }
    }
    fetchPaymentStatus();
});