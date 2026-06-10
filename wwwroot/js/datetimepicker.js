// Small custom date/time picker. It avoids native date inputs so display can follow hr/en browser language.
(function () {
    function pad(n) {
        return String(n).padStart(2, '0');
    }

    function browserCulture() {
        var lang = (navigator.languages && navigator.languages[0]) || navigator.language || 'hr';
        return lang.toLowerCase().indexOf('hr') === 0 ? 'hr' : 'en';
    }

    function parseIso(value) {
        if (!value) return null;
        var m = String(value).match(/^(\d{4})-(\d{2})-(\d{2})(?:T(\d{2}):(\d{2}))?/);
        if (!m) return null;
        return {
            year: parseInt(m[1], 10),
            month: parseInt(m[2], 10),
            day: parseInt(m[3], 10),
            hour: parseInt(m[4] || '0', 10),
            minute: parseInt(m[5] || '0', 10)
        };
    }

    function toIso(parts, showTime) {
        if (!parts) return '';
        var date = parts.year + '-' + pad(parts.month) + '-' + pad(parts.day);
        return showTime ? date + 'T' + pad(parts.hour || 0) + ':' + pad(parts.minute || 0) : date;
    }

    function formatDisplay(parts, showTime) {
        if (!parts) return '';
        var culture = browserCulture();
        var date = culture === 'hr'
            ? pad(parts.day) + '.' + pad(parts.month) + '.' + parts.year + '.'
            : pad(parts.month) + '/' + pad(parts.day) + '/' + parts.year;
        return showTime ? date + ' ' + pad(parts.hour || 0) + ':' + pad(parts.minute || 0) : date;
    }

    function daysInMonth(year, month) {
        return new Date(year, month, 0).getDate();
    }

    function partsFromDate(date) {
        return {
            year: date.getFullYear(),
            month: date.getMonth() + 1,
            day: date.getDate(),
            hour: date.getHours(),
            minute: date.getMinutes()
        };
    }

    function syncInput($input) {
        var hiddenId = $input.data('hidden-id');
        var showTime = $input.data('show-time') !== false && $input.data('show-time') !== 'false';
        var parts = parseIso($('#' + hiddenId).val());
        $input.val(formatDisplay(parts, showTime));
    }

    function renderPicker($input) {
        $('.datetime-picker-popup').remove();

        var hiddenId = $input.data('hidden-id');
        var showTime = $input.data('show-time') !== false && $input.data('show-time') !== 'false';
        var parts = parseIso($('#' + hiddenId).val()) || partsFromDate(new Date());
        var maxDay = daysInMonth(parts.year, parts.month);
        if (parts.day > maxDay) parts.day = maxDay;

        var $popup = $('<div class="datetime-picker-popup" />');
        var html = '';
        html += '<div class="datetime-picker-grid">';
        html += '<label>Day<input type="number" min="1" max="' + maxDay + '" data-part="day" value="' + parts.day + '"></label>';
        html += '<label>Month<input type="number" min="1" max="12" data-part="month" value="' + parts.month + '"></label>';
        html += '<label>Year<input type="number" min="1900" max="2100" data-part="year" value="' + parts.year + '"></label>';
        if (showTime) {
            html += '<label>Hour<input type="number" min="0" max="23" data-part="hour" value="' + parts.hour + '"></label>';
            html += '<label>Minute<input type="number" min="0" max="59" data-part="minute" value="' + parts.minute + '"></label>';
        }
        html += '</div>';
        html += '<div class="datetime-picker-actions"><button type="button" class="btn app-btn app-btn-muted datetime-now">Now</button><button type="button" class="btn app-btn app-btn-primary datetime-apply">Apply</button></div>';
        $popup.html(html);

        $('body').append($popup);
        var offset = $input.offset();
        $popup.css({
            top: offset.top + $input.outerHeight() + 6,
            left: offset.left,
            minWidth: $input.outerWidth()
        }).fadeIn(120);

        function collect() {
            var next = $.extend({}, parts);
            $popup.find('[data-part]').each(function () {
                var key = $(this).data('part');
                next[key] = parseInt($(this).val(), 10) || 0;
            });
            next.month = Math.min(12, Math.max(1, next.month));
            next.day = Math.min(daysInMonth(next.year, next.month), Math.max(1, next.day));
            next.hour = Math.min(23, Math.max(0, next.hour || 0));
            next.minute = Math.min(59, Math.max(0, next.minute || 0));
            return next;
        }

        $popup.on('click', '.datetime-now', function () {
            parts = partsFromDate(new Date());
            $('#' + hiddenId).val(toIso(parts, showTime));
            syncInput($input);
            $popup.fadeOut(100, function () { $popup.remove(); });
        });

        $popup.on('click', '.datetime-apply', function () {
            parts = collect();
            $('#' + hiddenId).val(toIso(parts, showTime));
            syncInput($input);
            $popup.fadeOut(100, function () { $popup.remove(); });
        });
    }

    $(function () {
        $('.datetime-picker').each(function () {
            syncInput($(this));
        });

        $(document).on('focus click', '.datetime-picker', function () {
            renderPicker($(this));
        });

        $(document).on('click', function (e) {
            if ($(e.target).closest('.datetime-picker-control, .datetime-picker-popup').length === 0) {
                $('.datetime-picker-popup').fadeOut(100, function () { $(this).remove(); });
            }
        });

        $(document).on('input', '.datetime-picker', function () {
            if ($(this).val() === '') {
                $('#' + $(this).data('hidden-id')).val('');
            }
        });
    });
})();
