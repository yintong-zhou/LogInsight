$(document).ready(function () {
    var logTable = new DataTable('#log-table',
        {
            "dom": "",
            "order": [[1, 'desc']],
            "paging": true,
            "searching": true,
            "ordering": true,
            "responsive": true,
        }
    );

    // Custom filtering function
    $.fn.dataTable.ext.search.push(
        function (settings, data, dataIndex) {
            var selectedLevel = $('#level').val().toUpperCase();
            var selectedDate = $('#date').val();
            // data[1] is the Level column
            var rowLevel = data[0];
            var rowDate = data[1]; // Time column

            // Date formatting for comparison
            var rowDateFormatted = rowDate.split(' ')[0]; // Get only the date part
            // Level filter
            var levelMatch = (selectedLevel === "" || rowLevel.indexOf(selectedLevel) >= 0);

            // Date filter
            var selectedDateParts = selectedDate.split('-');  // yyyy-MM-dd
            var selectedDateFormat = selectedDateParts[2] + '/' + selectedDateParts[1] + '/' + selectedDateParts[0];
            var dateMatch = (selectedDate === "" || rowDateFormatted === selectedDateFormat);
            console.log(rowDateFormatted, selectedDate, dateMatch);

            return levelMatch && dateMatch;
        }
    );

    // Event listener for the filter button
    $('#filter').on('click', function () {
        logTable.draw(); // Redraw the table with the new filter
    });

    // Event listener for the reset button
    $('#reset').on('click', function () {
        $('#level').val('');
        $('#date').val('');
        logTable.search('').draw(); // Clear all filters and redraw
    });

    $('#export').on('click', function () {
        let csvContent = "";
        let table = $('#log-table'); 

        // --- 1. Get Headers ---
        let headers = [];
        table.find('thead th').each(function () {
            headers.push(escapeCsvValue($(this).text().trim())); // Pulisce e escapa il testo dell'header
        });
        csvContent += headers.join(',') + '\n'; // Unisce gli header con virgole e aggiunge newline

        // --- 2. Get Data from Visible Rows ---
        table.find('tbody tr:visible').each(function () {
            let rowData = [];
            $(this).find('td').each(function (index) {
                let cellText;

                // If the cell contains a link, get the text of the link
                if (index === 0 && $(this).find('span.badge').length > 0) {
                    cellText = $(this).find('span.badge').text().trim(); 
                } else {
                    cellText = $(this).text().trim(); // Normal cell text
                }
                rowData.push(escapeCsvValue(cellText)); // Clear and escape the cell text
            });
            csvContent += rowData.join(',') + '\n';
        });

        // --- 3. create and download file ---
        if (csvContent === headers.join(',') + '\n') {
            alert("No data to export."); 
            return;
        }

        // Create a blob object with the CSV content
        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });

        // temporary link element for download
        const link = document.createElement('a');

        if (link.download !== undefined) {
            const url = URL.createObjectURL(blob);
            link.setAttribute('href', url);
            link.setAttribute('download', 'logs.csv'); 
            link.style.visibility = 'hidden'; 
            document.body.appendChild(link); 
            link.click(); 
            document.body.removeChild(link); 
            URL.revokeObjectURL(url); 
        } else {
            alert("Your browser does not support download. Try update it or use different browser");
            // window.open('data:text/csv;charset=utf-8,' + encodeURIComponent(csvContent));
        }
    });
});

function escapeCsvValue(value) {
    if (value == null) { 
        return '';
    }
    value = String(value); 

    // If contains double quotes, commas, or newlines, escape it
    if (value.includes(',') || value.includes('"') || value.includes('\n') || value.includes('\r')) {
        value = value.replace(/"/g, '""');
        value = '"' + value + '"';
    }
    return value;
}
