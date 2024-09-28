const ChartColor = ["#5D62B4", "#54C3BE", "#EF726F", "#F9C446", "rgb(93, 98, 180)", "#21B7EC", "#04BCCC"];
const primaryColor = getComputedStyle(document.body).getPropertyValue('--primary');
const secondaryColor = getComputedStyle(document.body).getPropertyValue('--secondary');
const successColor = getComputedStyle(document.body).getPropertyValue('--success');
const warningColor = getComputedStyle(document.body).getPropertyValue('--warning');
const dangerColor = getComputedStyle(document.body).getPropertyValue('--danger');
const infoColor = getComputedStyle(document.body).getPropertyValue('--info');
const darkColor = getComputedStyle(document.body).getPropertyValue('--dark');
const lightColor = getComputedStyle(document.body).getPropertyValue('--light');

(function ($) {
    'use strict';

    $(function () {
        const body = $('body');
        const sidebar = $('.sidebar');
        const current = location.pathname.split("/").slice(-1)[0].replace(/^\/|\/$/g, '');

        // Function to add active class to nav links
        function addActiveClass(element) {
            const isRootUrl = current === "";
            const isActive = isRootUrl
                ? element.attr('href').includes("index.html")
                : element.attr('href').includes(current);

            if (isActive) {
                const $navItem = element.parents('.nav-item').last();
                $navItem.addClass('active');

                if (element.parents('.sub-menu').length) {
                    element.closest('.collapse').addClass('show');
                    element.addClass('active');
                }

                if (element.parents('.submenu-item').length) {
                    element.addClass('active');
                }
            }
        }

        // Add active class to sidebar and horizontal menu items
        sidebar.find('.nav li a').each(function () {
            addActiveClass($(this));
        });

        $('.horizontal-menu .nav li a').each(function () {
            addActiveClass($(this));
        });

        // Close other submenus in sidebar on opening any
        sidebar.on('show.bs.collapse', function () {
            sidebar.find('.collapse.show').collapse('hide');
        });

        // Apply styles to sidebar
        applyStyles();

        function applyStyles() {
            if (!body.hasClass("rtl") && body.hasClass("sidebar-fixed")) {
                new PerfectScrollbar('#sidebar .nav');
            }
        }

        // Toggle sidebar visibility
        $('[data-toggle="minimize"]').on("click", function () {
            const isToggleDisplay = body.hasClass('sidebar-toggle-display') || body.hasClass('sidebar-absolute');
            body.toggleClass(isToggleDisplay ? 'sidebar-hidden' : 'sidebar-icon-only');
        });

        // Checkbox and radios
        $(".form-check label, .form-radio label").append('<i class="input-helper"></i>');

        // Fullscreen toggle
        $("#fullscreen-button").on("click", toggleFullScreen);

        function toggleFullScreen() {
            const isFullScreenActive = !!document.fullscreenElement || !!document.msFullscreenElement || !!document.mozFullScreen || !!document.webkitIsFullScreen;
            isFullScreenActive ? exitFullScreen() : enterFullScreen();
        }

        function enterFullScreen() {
            const elem = document.documentElement;
            if (elem.requestFullscreen) {
                elem.requestFullscreen();
            } else if (elem.mozRequestFullScreen) {
                elem.mozRequestFullScreen();
            } else if (elem.webkitRequestFullscreen) {
                elem.webkitRequestFullscreen(Element.ALLOW_KEYBOARD_INPUT);
            } else if (elem.msRequestFullscreen) {
                elem.msRequestFullscreen();
            }
        }

        function exitFullScreen() {
            if (document.exitFullscreen) {
                document.exitFullscreen();
            } else if (document.mozCancelFullScreen) {
                document.mozCancelFullScreen();
            } else if (document.webkitExitFullscreen) {
                document.webkitExitFullscreen();
            } else if (document.msExitFullscreen) {
                document.msExitFullscreen();
            }
        }

        // Handle purple free banner
        handleBanner();

        function handleBanner() {
            const proBanner = document.querySelector('#proBanner');
            const navbar = document.querySelector('.navbar');

            if ($.cookie('purple-free-banner') != "true") {
                proBanner.classList.add('d-flex');
                navbar.classList.remove('fixed-top');
            } else {
                proBanner.classList.add('d-none');
                navbar.classList.add('fixed-top');
            }

            updateNavbarPadding(navbar);

            document.querySelector('#bannerClose').addEventListener('click', function () {
                proBanner.classList.add('d-none');
                proBanner.classList.remove('d-flex');
                navbar.classList.remove('pt-5');
                navbar.classList.add('fixed-top');
                document.querySelector('.page-body-wrapper').classList.add('proBanner-padding-top');
                navbar.classList.remove('mt-3');
                setCookie('purple-free-banner', "true");
            });
        }

        function updateNavbarPadding(navbar) {
            const bodyWrapper = document.querySelector('.page-body-wrapper');
            if (navbar.classList.contains("fixed-top")) {
                bodyWrapper.classList.remove('pt-0');
                navbar.classList.remove('pt-5');
            } else {
                bodyWrapper.classList.add('pt-0');
                navbar.classList.add('pt-5', 'mt-3');
            }
        }

        function setCookie(name, value) {
            const date = new Date();
            date.setTime(date.getTime() + 24 * 60 * 60 * 1000);
            $.cookie(name, value, { expires: date });
        }
    });

})(jQuery);
