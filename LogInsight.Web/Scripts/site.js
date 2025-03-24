$(document).ready(function () {
    var logTable = new DataTable('#log-table',
        {
            "dom": "",
            "order": [[0, 'desc']],
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
            var rowLevel = data[1];
            var rowDate = data[0]; // Time column

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
});

