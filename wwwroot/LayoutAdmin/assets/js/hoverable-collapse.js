(function ($) {
    'use strict';

    // Open submenu on hover in compact sidebar mode and horizontal menu mode
    $(document).on('mouseenter mouseleave', '.sidebar .nav-item', function (ev) {
        const body = $('body');
        const sidebarIconOnly = body.hasClass("sidebar-icon-only");
        const sidebarFixed = body.hasClass("sidebar-fixed");

        // Early exit if touch is supported
        if ('ontouchstart' in document.documentElement) return;

        // Handle hover events based on sidebar state
        if (sidebarIconOnly) {
            handleIconOnlySidebar(ev, body, sidebarFixed);
        }
    });

    function handleIconOnlySidebar(ev, body, sidebarFixed) {
        if (sidebarFixed && ev.type === 'mouseenter') {
            body.removeClass('sidebar-icon-only');
        } else if (!sidebarFixed) {
            toggleMenuItemHover(ev, $(this));
        }
    }

    function toggleMenuItemHover(ev, $menuItem) {
        if (ev.type === 'mouseenter') {
            $menuItem.addClass('hover-open');
        } else {
            $menuItem.removeClass('hover-open');
        }
    }

})(jQuery);
