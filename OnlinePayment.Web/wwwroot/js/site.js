function getBaseUrl() {
    let url = new URL(window.location.href);
    let pathname = getPathName(url);
    url = `${url.protocol}//${url.host}${pathname}`;
    if (url.substring(url.length - 1) === "/") {
        url = url.slice(0, -1);
    }
    url += "/resources";
    return url;
}

function getPathName(url) {
    let pathName = url.pathname;
    let pathNames = pathName.split('/');
    if (pathNames.length > 2) {
        pathName = '/' + pathNames[1];
    }
    return pathName;
};

$(document).ready(function () {
    $('.tooltip-r').tooltip();

    $('#iconLanguageId').on('click', function () {
        $('#changeLanguageFormId').submit();
    })
    let url = getBaseUrl();
    $.get(url, function (json) {
        let data = JSON.parse(json);
        $('.data-table').DataTable({
            'language': {
                'url': data.TableLang
            },
            "paging": false,
            "order": [],
            dom: 'Bfrtip',
            buttons: [
                { extend: 'copy', className: 'btn btn-secondary btn-sm table-button' },
                { extend: 'excel', className: 'btn btn-secondary btn-sm table-button' },
                { extend: 'csv', className: 'btn btn-secondary btn-sm table-button' },
                { extend: 'print', className: 'btn btn-secondary btn-sm table-button' }
            ],
            initComplete: function () {
                $(".table-button").removeClass("buttons-html5").removeClass("buttons-copy").removeClass("dt-button");
            },

        });
    });
    let applicationName = $('#applicationName').val();
    let applicationUrlPart = getApplicationUrlPart(applicationName);
    let intervalId = setInterval(fetchPaymentStatus, 3000);
    document.getElementById('loadingSpinner').style.display = 'block';

    function fetchPaymentStatus() {
        let status = $('#status').val();
        if (status === 'PAID' ||
            status === 'ERROR' ||
            status === 'CANCELLED' ||
            status === 'DECLINED') {

            clearInterval(intervalId);
            window.location.href = `${window.location.origin}/${applicationUrlPart}${status.toLowerCase()}`;

        } else {
            let baseUrl = window.location.origin;
            let session = $('#session').val();
            let endpoint = `${baseUrl}/${applicationUrlPart}api/v1/payments/session/${session}`;
            $.ajax({
                url: endpoint,
                method: 'GET',
                success: function (response) {
                    if (response && response.status) {
                        if (response && response.status === 'PAID') {
                            $('#status').val(response.status);
                            changeSpinner('success');
                        }
                    }
                },
                error: function (xhr, status, error) {
                    console.error('Error fetching payment:', error);
                    changeSpinner('error');
                }
            });
        }

        function changeSpinner(msg) {
            let spinner = document.getElementById('loadingSpinner');
            spinner.classList.remove('spinner');
            spinner.classList.add(msg);
        }
    }

    function getApplicationUrlPart(applicationName) {
        let applicationUrlPart = '';
        if (applicationName && applicationName.trim() !== '') {
            applicationUrlPart = `${applicationName}/`;
        }
        return applicationUrlPart;
    }

    if (window.location.href.includes('pay')) {
        fetchPaymentStatus();
    }

});