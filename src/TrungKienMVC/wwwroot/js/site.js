// Popup (Bootstrap modal) helper used by every Create / Edit / Delete / Details action.
//
// - Elements with [data-modal-url] load a partial view into #appModal.
// - Forms marked [data-ajax-form] inside the modal are posted with AJAX:
//     * JSON { success: true }            -> close the modal and reload the page
//     * JSON { success: false, message }  -> show the message inside the modal
//     * HTML                              -> validation errors; re-render the form
//
// The focus contract this file is responsible for, none of which Bootstrap 5.1 gives us:
//   * focus moves INTO the dialog when it opens (without it, Escape never reaches the
//     dialog's own keydown handler, so the dialog cannot be dismissed from the keyboard);
//   * Tab cycles inside the dialog instead of walking the page behind it;
//   * focus returns to the control that opened it when the dialog closes.
(function ($) {
    'use strict';

    const modalEl = document.getElementById('appModal');
    if (!modalEl) return;

    const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
    const $content = $(modalEl).find('.modal-content');
    let lastTrigger = null;

    const FOCUSABLE = 'a[href], button:not([disabled]), input:not([type="hidden"]):not([disabled]),' +
        ' select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])';

    function focusables() {
        return $content.find(FOCUSABLE).filter(':visible').toArray();
    }

    // Land on the control the user actually came for:
    //   1. the first field that failed validation, so the error is announced with the field;
    //   2. otherwise the first form field — a form is for filling in;
    //   3. otherwise Cancel/Close, so a delete confirmation opens on the safe action and
    //      never on the destructive one;
    //   4. otherwise the first control (the header's close button), then the dialog itself.
    function focusFirst() {
        const invalid = $content.find('.input-validation-error').filter(':visible').first();
        if (invalid.length) {
            invalid.trigger('focus');
            return;
        }
        const field = $content.find('form')
            .find('input:not([type="hidden"]), select, textarea')
            .filter(':visible').first();
        if (field.length) {
            field.trigger('focus');
            return;
        }
        const dismiss = $content.find('.modal-footer [data-bs-dismiss]').filter(':visible').first();
        if (dismiss.length) {
            dismiss.trigger('focus');
            return;
        }
        const items = focusables();
        if (items.length) items[0].focus();
        else modalEl.focus();
    }

    function render(html) {
        $content.html(html);
        const $form = $content.find('form');
        if ($form.length && $.validator && $.validator.unobtrusive) {
            $form.removeData('validator').removeData('unobtrusiveValidation');
            $.validator.unobtrusive.parse($form);
        }
        focusFirst();
    }

    function showError(message) {
        render('<div class="modal-header">' +
            '<h2 class="modal-title h5" id="appModalTitle">Something went wrong</h2>' +
            '<button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close dialog"></button>' +
            '</div>' +
            '<div class="modal-body"><div class="alert alert-danger mb-0" role="alert"></div></div>' +
            '<div class="modal-footer"><button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button></div>');
        $content.find('.alert').text(message);
        focusFirst();
    }

    function showLoading() {
        render('<div class="modal-header">' +
            '<h2 class="modal-title h5" id="appModalTitle">Loading</h2>' +
            '</div>' +
            '<div class="modal-body"><p class="muted mb-0" role="status">Fetching the record…</p></div>');
    }

    // Capture phase, so the cycle happens before the browser moves focus out of the dialog.
    function onKeydown(event) {
        if (event.key === 'Escape') {
            event.preventDefault();
            modal.hide();
            return;
        }
        if (event.key !== 'Tab') return;

        const items = focusables();
        if (!items.length) {
            event.preventDefault();
            modalEl.focus();
            return;
        }
        const first = items[0];
        const last = items[items.length - 1];
        const active = document.activeElement;

        if (!modalEl.contains(active)) {
            event.preventDefault();
            first.focus();
        } else if (event.shiftKey && active === first) {
            event.preventDefault();
            last.focus();
        } else if (!event.shiftKey && active === last) {
            event.preventDefault();
            first.focus();
        }
    }

    window.openModal = function (url) {
        // Shown before the request lands: a dialog that stays blank for 400ms reads as broken.
        showLoading();
        modal.show();

        $.get(url)
            .done(function (html) { render(html); })
            .fail(function (xhr) {
                showError(xhr.status === 404
                    ? 'That record no longer exists — it may have been deleted.'
                    : 'Could not load the record (' + xhr.status + ').');
            });
    };

    $(document).on('click', '[data-modal-url]', function (e) {
        e.preventDefault();
        lastTrigger = this;
        window.openModal($(this).data('modal-url'));
    });

    modalEl.addEventListener('shown.bs.modal', function () {
        document.addEventListener('keydown', onKeydown, true);
        focusFirst();
    });

    modalEl.addEventListener('hidden.bs.modal', function () {
        document.removeEventListener('keydown', onKeydown, true);
        // Focus returns to the control that opened the dialog, not to the top of the page.
        if (lastTrigger && document.contains(lastTrigger)) {
            lastTrigger.focus();
        }
        lastTrigger = null;
    });

    $(document).on('submit', '#appModal form[data-ajax-form]', function (e) {
        e.preventDefault();
        const $form = $(this);
        if ($form.valid && !$form.valid()) {
            focusFirst();
            return;
        }

        const $buttons = $form.find('button[type=submit]');
        $buttons.prop('disabled', true).attr('aria-busy', 'true');

        function release() {
            $buttons.prop('disabled', false).removeAttr('aria-busy');
        }

        $.ajax({ url: $form.attr('action'), type: 'POST', data: $form.serialize() })
            .done(function (result) {
                if (result && typeof result === 'object') {
                    if (result.success) {
                        modal.hide();
                        window.location.reload();
                        return;
                    }
                    const $alert = $form.find('.ajax-error');
                    $alert.text(result.message || 'That change could not be saved.').removeClass('d-none');
                    release();
                    $alert.trigger('focus');
                } else {
                    // The server returned the form again, with validation errors in it.
                    render(result);
                }
            })
            .fail(function (xhr) {
                showError(xhr.status === 404
                    ? 'That record no longer exists — it may have been deleted.'
                    : 'The server could not complete the request (' + xhr.status + '). Try again.');
            });
    });
})(jQuery);
