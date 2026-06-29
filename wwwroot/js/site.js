// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Relax jQuery Validate URL rule to accept localhost and common dev hosts (keeps server-side validation intact)
$(function () {
	if (window.$ && $.validator && $.validator.methods) {
		$.validator.methods.url = function (value, element) {
			if (this.optional(element)) return true;
			var re = /^(https?:\/\/)((localhost(:\d+)?)|(([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}))(:\d+)?(\/.*)?$/i;
			return re.test(value);
		};
	}
});
// Trigger jQuery Validation on blur for form fields to provide immediate feedback.
$(function () {
	if (typeof $ === 'undefined' || !$.validator) return;
	$(document).on('blur', 'form input, form textarea, form select', function () {
		var $field = $(this);
		if ($field.closest('form').length === 0) return;
		try {
			$field.valid();
		} catch (e) {
			// ignore if validation plugin not ready
		}
	});
});

$(function () {
	$(document).on('click', '.app-menu-toggle', function () {
		var $button = $(this);
		var $menu = $('#appMenu');
		var open = !$menu.hasClass('open');
		$menu.toggleClass('open', open);
		$button.attr('aria-expanded', String(open));
	});

	$(document).on('click', '#generateRequestDraft', function () {
		var $button = $(this);
		var prompt = String($('#aiRequestPrompt').val() || '').trim();
		var $status = $('#aiRequestStatus');
		if (!prompt) {
			$status.text('Describe the request you want to create.');
			return;
		}

		$button.prop('disabled', true);
		$status.text('Generating draft...');
		$.ajax({
			url: '/api/ai/request-draft',
			method: 'POST',
			contentType: 'application/json',
			data: JSON.stringify({ prompt: prompt })
		}).done(function (draft) {
			$('#requestName').val(draft.name || draft.Name || '').trigger('input');
			$('#requestMethod').val(draft.method !== undefined ? draft.method : draft.Method).trigger('change');
			$('#requestUrl').val(draft.url || draft.Url || '').trigger('input');
			$('#Body').val(draft.body || draft.Body || '').trigger('input');

			var headers = draft.headers || draft.Headers || {};
			var $list = $('#headerEditorList').empty();
			var index = 0;
			Object.keys(headers).forEach(function (key) {
				var row = '<div class="header-editor-row">'
					+ '<input type="hidden" name="Headers[' + index + '].RequestId" value="0" />'
					+ '<input name="Headers[' + index + '].Key" class="form-control" maxlength="150" />'
					+ '<input name="Headers[' + index + '].Value" class="form-control" maxlength="2000" />'
					+ '<label class="form-check"><input name="Headers[' + index + '].IsEnabled" value="true" type="checkbox" class="form-check-input" checked />'
					+ '<input name="Headers[' + index + '].IsEnabled" value="false" type="hidden" /><span>On</span></label>'
					+ '<button type="button" class="btn app-btn app-btn-muted remove-header-row">Remove</button></div>';
				var $row = $(row);
				$row.find('input[name$=".Key"]').val(key);
				$row.find('input[name$=".Value"]').val(headers[key]);
				$list.append($row);
				index++;
			});
			$('.add-header-row').attr('data-next-index', index);
			$status.text(draft.message || draft.Message || 'Draft generated. Review it before saving or sending.');
		}).fail(function (xhr) {
			$status.text(xhr.responseText || 'The request draft could not be generated.');
		}).always(function () {
			$button.prop('disabled', false);
		});
	});
});

// Debounced AJAX list search for collections, workspaces and environments with fade animation
(function () {
	var timers = {};

	function escapeHtml(s) {
		return String(s).replace(/[&<>\"'`]/g, function (ch) {
			return ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;', '`': '&#96;' })[ch];
		});
	}

	function pick(obj, pascal, camel, fallback) {
		if (!obj) return fallback;
		if (obj[camel] !== undefined && obj[camel] !== null) return obj[camel];
		if (obj[pascal] !== undefined && obj[pascal] !== null) return obj[pascal];
		return fallback;
	}

	// Inject HTML into a modal body, re-parse unobtrusive validation, and show the modal
	function injectModalHtml(selector, html) {
		var $body = $(selector);
		$body.html(html);
		try {
			if (typeof $ !== 'undefined' && $.validator && $.validator.unobtrusive && typeof $.validator.unobtrusive.parse === 'function') {
				$.validator.unobtrusive.parse($body);
			}
		} catch (e) {
			console.warn('unobtrusive.parse failed for', selector, e);
		}
		var $modal = $body.closest('.modal');
		if ($modal.length && window.bootstrap) {
			var modalEl = $modal.get(0);
			var modal = new bootstrap.Modal(modalEl);
			modal.show();
		}
	}

	window.injectModalHtml = injectModalHtml;

	function parseValidation($scope) {
		try {
			if (typeof $ !== 'undefined' && $.validator && $.validator.unobtrusive && typeof $.validator.unobtrusive.parse === 'function') {
				$.validator.unobtrusive.parse($scope);
			}
		} catch (e) {
			console.warn('validation parse failed', e);
		}
	}

	function replaceAjaxForm($form, html) {
		var $modalBody = $form.closest('.modal-body');
		if ($modalBody.length) {
			$modalBody.html(html);
			parseValidation($modalBody);
			$modalBody.find('.datetime-picker').each(function () {
				$(this).trigger('input');
			});
			return;
		}

		var $incoming = $('<div>').html(html);
		var $newForm = $incoming.find('form').first();
		if ($newForm.length) {
			$form.replaceWith($newForm);
			parseValidation($newForm);
		}
	}

	function renderCollections(items) {
		var $grid = $('.collections-grid');
		if (!$grid.length) return;
		if (!items || items.length === 0) {
			$grid.fadeOut(150, function () {
				$grid.html('<p class="text-muted">No collections found.</p>').fadeIn(150);
			});
			return;
		}

		var html = '';
		items.forEach(function (c) {
			var id = pick(c, 'Id', 'id', '');
			var name = pick(c, 'Name', 'name', '');
			var description = pick(c, 'Description', 'description', '');
			var isShared = pick(c, 'IsShared', 'isShared', false);
			var workspace = pick(c, 'Workspace', 'workspace', '');
			var requests = pick(c, 'Requests', 'requests', 0);
			html += '<article class="collection-card" data-shared="' + (isShared ? 'true' : 'false') + '">';
			html += '<div class="collection-head"><h2>' + escapeHtml(name) + '</h2>';
			html += '<span class="collection-badge">' + (isShared ? 'Shared' : 'Private') + '</span></div>';
			html += '<p class="collection-description">' + escapeHtml(description || '') + '</p>';
			html += '<div class="collection-meta">';
			html += '<p><span>Workspace</span><strong>' + escapeHtml(workspace || '') + '</strong></p>';
			html += '<p><span>Requests</span><strong>' + (requests || 0) + '</strong></p>';
			html += '</div>';
			html += '<a class="entity-link" href="/collections/' + id + '">Open Collection Details</a>';
			html += '<a href="#" class="entity-link open-collection-edit" data-collection-id="' + id + '">Edit</a>';
			html += '<a href="#" class="entity-link open-collection-delete" data-collection-id="' + id + '">Delete</a>';
			html += '</article>';
		});

		$grid.fadeOut(150, function () {
			$grid.html(html).fadeIn(200, function () {
				$('.collection-filter-group .filter-chip.active').trigger('click');
			});
		});
	}

	function renderWorkspaces(items) {
		var $grid = $('.workspaces-grid');
		if (!$grid.length) return;
		if (!items || items.length === 0) {
			$grid.fadeOut(150, function () {
				$grid.html('<p class="text-muted">No workspaces found.</p>').fadeIn(150);
			});
			return;
		}

		var html = '';
		items.forEach(function (w) {
			var id = pick(w, 'Id', 'id', '');
			var name = pick(w, 'Name', 'name', '');
			var description = pick(w, 'Description', 'description', '');
			var collections = pick(w, 'Collections', 'collections', 0);
			var environments = pick(w, 'Environments', 'environments', 0);
			html += '<article class="workspace-card">';
			html += '<div class="workspace-head"><h2>' + escapeHtml(name) + '</h2></div>';
			html += '<p class="workspace-description">' + escapeHtml(description || '') + '</p>';
			html += '<div class="workspace-metrics">';
			html += '<p><span>Collections</span><strong>' + (collections || 0) + '</strong></p>';
			html += '<p><span>Environments</span><strong>' + (environments || 0) + '</strong></p>';
			html += '</div>';
			html += '<a href="#" class="entity-link open-workspace-details" data-workspace-id="' + id + '">Open Workspace Details</a>';
			html += '<a href="#" class="entity-link open-workspace-edit" data-workspace-id="' + id + '">Edit</a>';
			html += '<a href="#" class="entity-link open-workspace-delete" data-workspace-id="' + id + '">Delete</a>';
			html += '</article>';
		});

		$grid.fadeOut(150, function () {
			$grid.html(html).fadeIn(200);
		});

		// rebind handlers for dynamic content
		$('.open-workspace-details').off('click').on('click', function (e) {
			e.preventDefault();
			var id = $(this).data('workspace-id');
			var url = '/workspace-details?workspaceId=' + id;
			$.get(url, function (html) {
				if (window.injectModalHtml) window.injectModalHtml('#workspaceDetailsModal .modal-body', html);
				else { $('#workspaceDetailsModal .modal-body').html(html); var modalEl = document.getElementById('workspaceDetailsModal'); if (modalEl && window.bootstrap) { var modal = new bootstrap.Modal(modalEl); modal.show(); } }
			}).fail(function () { alert('Failed to load workspace details'); });
		});

		$('.open-workspace-edit').off('click').on('click', function (e) {
			e.preventDefault();
			var id = $(this).data('workspace-id');
			var url = '/workspaces/edit/' + id;
			$.get(url, function (html) {
				if (window.injectModalHtml) window.injectModalHtml('#workspaceEditModal .modal-body', html);
				else { $('#workspaceEditModal .modal-body').html(html); var modalEl = document.getElementById('workspaceEditModal'); if (modalEl && window.bootstrap) { var modal = new bootstrap.Modal(modalEl); modal.show(); } }
			}).fail(function () { alert('Failed to load edit form'); });
		});

		$('.open-workspace-delete').off('click').on('click', function (e) {
			e.preventDefault();
			var id = $(this).data('workspace-id');
			var form = $('#workspaceDeleteForm');
			form.attr('action', '/Workspaces/delete/' + id);
			var modalEl = document.getElementById('workspaceDeleteModal');
			if (modalEl && window.bootstrap) {
				var modal = new bootstrap.Modal(modalEl);
				modal.show();
			}
		});
	}

	function renderEnvironments(items) {
		var $list = $('.panel-list');
		if (!$list.length) return;
		if (!items || items.length === 0) {
			$list.fadeOut(150, function () {
				$list.html('<p class="text-muted">No environments found.</p>').fadeIn(150);
			});
			return;
		}

		var html = '';
		items.forEach(function (e) {
			var id = pick(e, 'Id', 'id', '');
			var name = pick(e, 'Name', 'name', '');
			var baseUrl = pick(e, 'BaseUrl', 'baseUrl', pick(e, 'Description', 'description', ''));
			html += '<li>';
			html += '<strong>' + escapeHtml(name) + '</strong>';
			html += '<span>' + escapeHtml(baseUrl || '') + '</span>';
			html += '<a class="entity-link" href="/request-builder">Use in Builder</a>';
			html += '<a href="#" class="entity-link open-environment-edit" data-environment-id="' + id + '">Edit</a>';
			html += '<a href="#" class="entity-link open-environment-delete" data-environment-id="' + id + '">Delete</a>';
			html += '</li>';
		});

		$list.fadeOut(150, function () {
			$list.html(html).fadeIn(200);
		});

		// rebind edit/delete handlers
		$('.open-environment-edit').off('click').on('click', function (ev) {
			ev.preventDefault();
			var id = $(this).data('environment-id');
			var url = '/environments/edit/' + id;
			$.get(url, function (html) {
				if (window.injectModalHtml) window.injectModalHtml('#environmentEditModal .modal-body', html);
				else { $('#environmentEditModal .modal-body').html(html); var modalEl = document.getElementById('environmentEditModal'); if (modalEl && window.bootstrap) { var modal = new bootstrap.Modal(modalEl); modal.show(); } }
			}).fail(function () { alert('Failed to load edit form'); });
		});

		$('.open-environment-delete').off('click').on('click', function (ev) {
			ev.preventDefault();
			var id = $(this).data('environment-id');
			var form = $('#environmentDeleteForm');
			form.attr('action', '/environments/delete/' + id);
			var modalEl = document.getElementById('environmentDeleteModal');
			if (modalEl && window.bootstrap) {
				var modal = new bootstrap.Modal(modalEl);
				modal.show();
			}
		});
	}

	function renderRequests(items) {
		console.debug('renderRequests called', items);
		var $stream = $('.requests-stream');
		if (!$stream.length) return;
		if (!items || items.length === 0) {
			$stream.fadeOut(150, function () {
				$stream.html('<p class="text-muted">No requests found.</p>').fadeIn(150);
			});
			return;
		}

			var html = '';
			var token = $('input[name="__RequestVerificationToken"]').first().val() || '';
			items.forEach(function (r) {
			var id = pick(r, 'Id', 'id', '');
			var name = pick(r, 'Name', 'name', '');
			var url = pick(r, 'Url', 'url', pick(r, 'Description', 'description', ''));
			var collection = pick(r, 'Collection', 'collection', '');
			var method = String(pick(r, 'Method', 'method', 'get')).toLowerCase();

			html += '<article class="request-row">';
			html += '<div class="request-main">';
			html += '<span class="method-badge ' + escapeHtml(method) + '">' + escapeHtml(method.toUpperCase()) + '</span>';
			html += '<div class="request-details">';
			if (name) html += '<h3 class="request-name">' + escapeHtml(name) + '</h3>';
			html += '<p class="request-url">' + escapeHtml(url) + '</p>';
			html += '<p class="request-collection">Collection: ' + escapeHtml(collection) + '</p>';
			html += '</div></div>';
			html += '<a href="#" class="entity-link open-request-details" data-request-id="' + id + '">Open Request Details</a>';
			html += '<a href="#" class="entity-link open-request-builder" data-request-id="' + id + '">Edit</a>';
			html += '<form action="/request-builder/run/' + id + '" method="post" class="request-run-form">';
			if (token) html += '<input name="__RequestVerificationToken" type="hidden" value="' + escapeHtml(token) + '" />';
			html += '<button type="submit" class="btn app-btn app-btn-primary">Run Request</button>';
			html += '</form>';
			html += '<a href="#" class="entity-link open-request-delete" data-request-id="' + id + '">Delete</a>';
			html += '</article>';
		});

		$stream.fadeOut(150, function () {
			$stream.html(html).fadeIn(200);
		});

	}

	$(function () {
		$(document).on('submit', 'form[data-ajax-validation="true"]', function (e) {
			var $form = $(this);
			if ($form.data('submitting')) return;

			e.preventDefault();

			if ($.validator && !$form.valid()) {
				return;
			}

			$form.data('submitting', true);

			$.ajax({
				url: $form.attr('action') || window.location.href,
				method: ($form.attr('method') || 'post').toUpperCase(),
				data: $form.serialize(),
				headers: { 'X-Requested-With': 'XMLHttpRequest' }
			}).done(function (_html, _status, xhr) {
				var responseUrl = xhr && xhr.responseURL ? xhr.responseURL : '';
				var current = window.location.href.split('#')[0];
				if (responseUrl && responseUrl.split('#')[0] !== current) {
					window.location.href = responseUrl;
					return;
				}
				window.location.reload();
			}).fail(function (xhr) {
				if (xhr && xhr.responseText) {
					replaceAjaxForm($form, xhr.responseText);
				}
			}).always(function () {
				$form.removeData('submitting');
			});
		});

		$(document).on('input', '.nested-filter-input', function () {
			var $input = $(this);
			var term = String($input.val() || '').toLowerCase();
			var targetSelector = $input.data('filter-target');
			var rowSelector = $input.data('filter-row');
			var $target = $(targetSelector);
			if (!$target.length || !rowSelector) return;

			$target.find(rowSelector).each(function () {
				var $row = $(this);
				var text = $row.text().toLowerCase();
				var match = !term || text.indexOf(term) !== -1;
				if (match) {
					$row.stop(true, true).slideDown(120);
				} else {
					$row.stop(true, true).slideUp(120);
				}
			});
		});

		// collections search (existing)
		var $cInput = $('#collectionSearch');
		if ($cInput.length) {
			$cInput.on('input', function () {
				var q = $cInput.val() || '';
				if (timers.collections) clearTimeout(timers.collections);
				timers.collections = setTimeout(function () {
					$.getJSON('/collections/search', { q: q })
					.done(function (data) {
							renderCollections(data);
							// rebind modal handlers for new elements
							$('.open-collection-edit').off('click').on('click', function (e) {
								e.preventDefault();
								var id = $(this).data('collection-id');
								var url = '/collections/edit/' + id;
								$.get(url, function (html) {
									if (window.injectModalHtml) window.injectModalHtml('#collectionEditModal .modal-body', html);
									else { $('#collectionEditModal .modal-body').html(html); var modalEl = document.getElementById('collectionEditModal'); if (modalEl && window.bootstrap) { var modal = new bootstrap.Modal(modalEl); modal.show(); } }
								}).fail(function () { alert('Failed to load edit form'); });
							});

							$('.open-collection-delete').off('click').on('click', function (e) {
								e.preventDefault();
								var id = $(this).data('collection-id');
								var form = $('#collectionDeleteForm');
								form.attr('action', '/collections/delete/' + id);
								var modalEl = document.getElementById('collectionDeleteModal');
								if (modalEl && window.bootstrap) {
									var modal = new bootstrap.Modal(modalEl);
									modal.show();
								}
							});
						})
						.fail(function () { /* silent */ });
				}, 250);
			});
		}

		// workspace search
		var $wInput = $('#workspaceSearch');
		if ($wInput.length) {
			$wInput.on('input', function () {
				var q = $wInput.val() || '';
				if (timers.workspaces) clearTimeout(timers.workspaces);
				timers.workspaces = setTimeout(function () {
					$.getJSON('/workspaces/search', { q: q })
						.done(function (data) {
							renderWorkspaces(data);
						})
						.fail(function () { /* silent */ });
				}, 250);
			});
		}

		// environment search
		var $eInput = $('#environmentSearch');
		if ($eInput.length) {
			$eInput.on('input', function () {
				var q = $eInput.val() || '';
				if (timers.environments) clearTimeout(timers.environments);
				timers.environments = setTimeout(function () {
					$.getJSON('/environments/search', { q: q })
						.done(function (data) {
							renderEnvironments(data);
						})
						.fail(function () { /* silent */ });
				}, 250);
			});
		}

		// request search
		var $rInput = $('#requestSearch');
		if ($rInput.length) {
			$rInput.on('input', function () {
				var q = $rInput.val() || '';
				if (timers.requests) clearTimeout(timers.requests);
				timers.requests = setTimeout(function () {
					$.getJSON('/requests/search', { q: q })
						.done(function (data) {
							console.debug('requests/search responded', Array.isArray(data) ? data.length : typeof data, data);
							renderRequests(data);
						})
						.fail(function (jqxhr, textStatus, errorThrown) {
							console.error('requests/search failed', textStatus, errorThrown, jqxhr && jqxhr.responseText);
							var resp = jqxhr && jqxhr.responseText ? jqxhr.responseText : '';
							// If server returned HTML (likely a login redirect), send user to login page
							if (typeof resp === 'string' && (resp.indexOf('<!DOCTYPE') === 0 || resp.indexOf('<html') === 0 || resp.indexOf('<title>Log in') !== -1)) {
								var returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
								window.location.href = '/Identity/Account/Login?returnUrl=' + returnUrl;
								return;
							}
							$('.requests-stream').fadeOut(150, function () { $(this).html('<p class="text-muted">Search failed.</p>').fadeIn(150); });
						});
				}, 250);
			});
		}

		var $tagInput = $('#tagSearch');
		if ($tagInput.length) {
			$tagInput.on('input', function () {
				var q = $tagInput.val() || '';
				if (timers.tags) clearTimeout(timers.tags);
				timers.tags = setTimeout(function () {
					$.getJSON('/tags/search', { q: q })
						.done(function (items) {
							var $list = $('#tagList');
							if (!$list.length) return;
							if (!items || items.length === 0) {
								$list.fadeOut(150, function () { $list.html('<p class="text-muted">No tags found.</p>').fadeIn(150); });
								return;
							}
							var html = '';
							items.forEach(function (t) {
								var id = pick(t, 'Id', 'id', '');
								var name = pick(t, 'Name', 'name', '');
								var color = pick(t, 'ColorHex', 'colorHex', pick(t, 'Description', 'description', '#13D0D4'));
								html += '<article class="tag-row ajax-tag-row">';
								html += '<span class="tag-swatch" style="background:' + escapeHtml(color) + '"></span>';
								html += '<strong>' + escapeHtml(name) + '</strong>';
								html += '<span>' + escapeHtml(color) + '</span>';
								html += '<a class="entity-link" href="/tags">Edit on full list</a>';
								html += '</article>';
							});
							$list.fadeOut(150, function () { $list.html(html).fadeIn(200); });
						});
				}, 250);
			});
		}

		$(document).on('click', '.add-header-row', function () {
			var $button = $(this);
			var index = parseInt($button.attr('data-next-index'), 10) || 0;
			var html = '';
			html += '<div class="header-editor-row" style="display:none">';
			html += '<input type="hidden" name="Headers[' + index + '].Id" value="0" />';
			html += '<input type="hidden" name="Headers[' + index + '].RequestId" value="0" />';
			html += '<input name="Headers[' + index + '].Key" class="form-control" placeholder="Header key" maxlength="150" />';
			html += '<input name="Headers[' + index + '].Value" class="form-control" placeholder="Header value" maxlength="2000" />';
			html += '<label class="form-check"><input name="Headers[' + index + '].IsEnabled" value="true" type="checkbox" class="form-check-input" checked /><input name="Headers[' + index + '].IsEnabled" value="false" type="hidden" /><span>On</span></label>';
			html += '<button type="button" class="btn app-btn app-btn-muted remove-header-row">Remove</button>';
			html += '</div>';
			$('#headerEditorList').append(html).find('.header-editor-row:last').slideDown(140);
			$button.attr('data-next-index', index + 1);
		});

		$(document).on('click', '.remove-header-row', function () {
			$(this).closest('.header-editor-row').slideUp(120, function () { $(this).remove(); });
		});

		$(document).on('click', '.runner-tab', function () {
			var $button = $(this);
			var tab = $button.data('runner-tab');
			if (!tab) return;
			var $panel = $button.closest('.runner-editor-panel');
			$panel.find('.runner-tab').removeClass('active');
			$button.addClass('active');
			$panel.find('.runner-tab-pane').removeClass('active');
			$panel.find('[data-runner-pane="' + tab + '"]').addClass('active');
		});

		$(document).on('click', '.response-tab', function () {
			var $button = $(this);
			var tab = $button.data('response-tab');
			if (!tab) return;
			var $panel = $button.closest('.runner-right-panel');
			$panel.find('.response-tab').removeClass('active');
			$button.addClass('active');
			$panel.find('.response-pane').removeClass('active');
			$panel.find('[data-response-pane="' + tab + '"]').addClass('active');
		});

		$(document).on('click', '.collection-filter-group .filter-chip', function () {
			var $button = $(this);
			var filter = String($button.data('collection-filter') || 'all');
			$('.collection-filter-group .filter-chip').removeClass('active');
			$button.addClass('active');
			$('.collections-grid .collection-card').each(function () {
				var $card = $(this);
				var isShared = String($card.data('shared')) === 'true';
				var show = filter === 'all' || (filter === 'shared' && isShared) || (filter === 'private' && !isShared);
				$card.stop(true, true)[show ? 'fadeIn' : 'fadeOut'](140);
			});
		});

		function formatFileSize(bytes) {
			var size = Number(bytes || 0);
			if (size < 1024) return size + ' B';
			if (size < 1024 * 1024) return (size / 1024).toFixed(1) + ' KB';
			return (size / (1024 * 1024)).toFixed(1) + ' MB';
		}

		function renderAttachments(items) {
			var $list = $('#requestAttachmentList');
			if (!$list.length) return;
			if (!items || items.length === 0) {
				$list.fadeOut(120, function () {
					$list.html('<p class="text-muted">No attachments yet.</p>').fadeIn(120);
				});
				return;
			}

			var html = '';
			items.forEach(function (a) {
				var id = pick(a, 'Id', 'id', '');
				var fileName = pick(a, 'FileName', 'fileName', '');
				var filePath = pick(a, 'FilePath', 'filePath', '#');
				var contentType = pick(a, 'ContentType', 'contentType', '');
				var fileSize = pick(a, 'FileSize', 'fileSize', 0);
				html += '<article class="request-attachment-row" data-attachment-id="' + id + '">';
				html += '<a href="' + escapeHtml(filePath) + '" target="_blank" rel="noopener">' + escapeHtml(fileName) + '</a>';
				html += '<span>' + escapeHtml(contentType || 'file') + '</span>';
				html += '<span>' + escapeHtml(formatFileSize(fileSize)) + '</span>';
				html += '<button type="button" class="btn app-btn app-btn-danger delete-request-attachment" data-attachment-id="' + id + '">Delete</button>';
				html += '</article>';
			});

			$list.fadeOut(120, function () {
				$list.html(html).fadeIn(160);
			});
		}

		function loadRequestAttachments() {
			var $panel = $('.request-attachments-panel');
			if (!$panel.length) return;
			var requestId = $panel.data('request-id');
			if (!requestId) return;
			$.getJSON('/api/request-attachments', { requestId: requestId })
				.done(renderAttachments)
				.fail(function () {
					$('#requestAttachmentList').html('<p class="text-danger">Attachments could not be loaded.</p>');
				});
		}

		loadRequestAttachments();
		$(document).on('shown.bs.modal', '.modal', loadRequestAttachments);

		$(document).on('change', '#requestAttachmentInput', function () {
			var input = this;
			var requestId = $(input).data('request-id');
			if (!requestId || !input.files || input.files.length === 0) return;

			var files = Array.prototype.slice.call(input.files);
			var uploadNext = function () {
				var file = files.shift();
				if (!file) {
					input.value = '';
					loadRequestAttachments();
					$('.request-attachment-status').text('');
					return;
				}

				var data = new FormData();
				data.append('file', file);
				$('.request-attachment-status').text('Uploading ' + file.name + '...');
				$.ajax({
					url: '/api/request-attachments/' + requestId,
					method: 'POST',
					data: data,
					processData: false,
					contentType: false
				}).done(uploadNext)
					.fail(function (xhr) {
						var message = xhr && xhr.responseText ? xhr.responseText : 'Upload failed.';
						$('.request-attachment-status').text(message);
					});
			};

			uploadNext();
		});

		$(document).on('click', '.delete-request-attachment', function () {
			var $button = $(this);
			var id = $button.data('attachment-id');
			if (!id) return;
			$.ajax({ url: '/api/request-attachments/' + id, method: 'DELETE' })
				.done(function () {
					$button.closest('.request-attachment-row').slideUp(120, function () {
						$(this).remove();
						if ($('.request-attachment-row').length === 0) {
							renderAttachments([]);
						}
					});
				})
				.fail(function () {
					$('.request-attachment-status').text('Delete failed.');
				});
		});
	});
})();
