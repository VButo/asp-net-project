/* Simple reusable autocomplete control
   - Expects an input with class `autocomplete-input` and `data-search-url` attribute
   - A sibling hidden input with the id given in `data-hidden-id` will be populated with the selected Id
*/
(function () {
    function escapeHtml(s) {
        return String(s).replace(/[&<>"'`]/g, function (ch) {
            return ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;', '`': '&#96;' })[ch];
        });
    }

    function debounce(fn, wait) {
        var t;
        return function () {
            var ctx = this, args = arguments;
            clearTimeout(t);
            t = setTimeout(function () { fn.apply(ctx, args); }, wait);
        };
    }

    $(function () {
        // Trigger search
        $(document).on('input', '.autocomplete-input', debounce(function () {
            var $input = $(this);
            var url = $input.data('search-url');
            var q = ($input.val() || '').trim();
            var $suggest = $('#' + $input.attr('id') + '_suggestions');
            if (!url) return;

            // clear the linked hidden id whenever the user types to avoid stale/undefined values
            var hiddenId = $input.data('hidden-id') || $input.attr('id').replace('Input', 'Id');
            try { $('#' + hiddenId).val(''); } catch (e) { /* ignore if element missing */ }

            if (q.length === 0) {
                try { $('#' + hiddenId).val(''); } catch (e) { }
                $suggest.hide();
                return;
            }

            $.getJSON(url, { q: q }).done(function (items) {
                if (!items || items.length === 0) {
                    $suggest.hide();
                    return;
                }

                var html = '';
                items.forEach(function (it, idx) {
                    var name = it.Name || it.name || '';
                    var desc = it.Description || it.description || it.BaseUrl || '';
                    // Handle both PascalCase and camelCase JSON (Id vs id)
                    var idVal = (it.Id !== undefined && it.Id !== null) ? it.Id : (it.id !== undefined && it.id !== null ? it.id : '');
                    var dataId = (idVal === null || idVal === undefined || idVal === '') ? '' : String(idVal);
                    html += '<a href="#" class="list-group-item list-group-item-action autocomplete-item' + (idx===0? ' active' : '') + '" data-id="' + escapeHtml(dataId) + '" data-text="' + escapeHtml(name) + '">';
                    html += '<div class="fw-bold">' + escapeHtml(name) + '</div>';
                    if (desc) html += '<div class="small text-muted">' + escapeHtml(desc) + '</div>';
                    html += '</a>';
                });

                $suggest.html(html).stop(true, true).slideDown(120);
            }).fail(function () {
                $suggest.stop(true, true).slideUp(100);
            });
        }, 200));

        // mouse interactions
        $(document).on('mouseenter', '.autocomplete-item', function () {
            $('.autocomplete-item.active').removeClass('active');
            $(this).addClass('active');
        });

        $(document).on('click', '.autocomplete-item', function (e) {
            e.preventDefault();
            var $item = $(this);
            var $suggest = $item.closest('.autocomplete-suggestions');
            var inputId = $suggest.attr('id').replace('_suggestions', '');
            var $input = $('#' + inputId);
            var hiddenId = $input.data('hidden-id') || inputId.replace('Input', 'Id');
            var rawId = $item.attr('data-id');
            var id = '';
            if (rawId !== undefined && rawId !== null && rawId !== '') {
                var parsed = parseInt(rawId, 10);
                if (!isNaN(parsed)) id = parsed;
            }
            var text = $item.attr('data-text');
            try { $('#' + hiddenId).val(id); } catch (e) { }
            $input.val(text);
            $suggest.hide();
        });

        // keyboard navigation
        $(document).on('keydown', '.autocomplete-input', function (e) {
            var $input = $(this);
            var $suggest = $('#' + $input.attr('id') + '_suggestions');
            if (!$suggest.is(':visible')) return;

            var $items = $suggest.find('.autocomplete-item');
            if (!$items.length) return;

            var $active = $items.filter('.active').first();

            if (e.key === 'ArrowDown') {
                e.preventDefault();
                if (!$active.length) {
                    $items.first().addClass('active');
                } else {
                    var $next = $active.nextAll('.autocomplete-item').first();
                    if ($next.length) {
                        $active.removeClass('active');
                        $next.addClass('active');
                        // ensure visible
                        $next[0].scrollIntoView({ block: 'nearest' });
                    }
                }
            } else if (e.key === 'ArrowUp') {
                e.preventDefault();
                if (!$active.length) {
                    $items.last().addClass('active');
                } else {
                    var $prev = $active.prevAll('.autocomplete-item').first();
                    if ($prev.length) {
                        $active.removeClass('active');
                        $prev.addClass('active');
                        $prev[0].scrollIntoView({ block: 'nearest' });
                    }
                }
            } else if (e.key === 'Enter') {
                e.preventDefault();
                var $sel = $items.filter('.active').first();
                if ($sel.length) {
                    $sel.click();
                } else {
                    $items.first().click();
                }
            } else if (e.key === 'Escape') {
                $suggest.hide();
            }
        });

        // hide suggestions when clicking outside
        $(document).on('click', function (e) {
            if ($(e.target).closest('.autocomplete-control').length === 0) {
                $('.autocomplete-suggestions').hide();
            }
        });

        // Ensure no hidden inputs carry the literal string 'undefined' when submitting forms
        $(document).on('submit', 'form', function () {
            $(this).find('input[type="hidden"]').each(function () {
                try {
                    var $h = $(this);
                    var v = $h.val();
                    if (v === undefined || v === 'undefined') {
                        $h.val('');
                    }
                } catch (e) {
                    // ignore
                }
            });
        });
    });
})();
