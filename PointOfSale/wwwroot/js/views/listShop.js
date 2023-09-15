AOS.init({
    duration: 800,
    easing: 'slide'
});

var productos = [];

(function ($) {

    $("#text-area-products").hide();
    $("#btnTrash").hide();

    $(".less-button").on("click", function () {
        setValue(event.currentTarget, -1);
    })

    $(".pluss-button").on("click", function () {
        setValue(event.currentTarget, 1);
    })

    $("#btnTrash").on("click", function () {
        clean();
    })

    $("#btnCompras").on("click", function () {
        resumenVenta();
    })

    $(".btnClose").on("click", function () {
        $("#modalData").modal("hide")
    })

    "use strict";

    var isMobile = {
        Android: function () {
            return navigator.userAgent.match(/Android/i);
        },
        BlackBerry: function () {
            return navigator.userAgent.match(/BlackBerry/i);
        },
        iOS: function () {
            return navigator.userAgent.match(/iPhone|iPad|iPod/i);
        },
        Opera: function () {
            return navigator.userAgent.match(/Opera Mini/i);
        },
        Windows: function () {
            return navigator.userAgent.match(/IEMobile/i);
        },
        any: function () {
            return (isMobile.Android() || isMobile.BlackBerry() || isMobile.iOS() || isMobile.Opera() || isMobile.Windows());
        }
    };


    //$(window).stellar({
    //    responsive: true,
    //    parallaxBackgrounds: true,
    //    parallaxElements: true,
    //    horizontalScrolling: false,
    //    hideDistantElements: false,
    //    scrollProperty: 'scroll'
    //});


    var fullHeight = function () {

        $('.js-fullheight').css('height', $(window).height());
        $(window).resize(function () {
            $('.js-fullheight').css('height', $(window).height());
        });

    };
    fullHeight();

    // loader
    var loader = function () {
        setTimeout(function () {
            if ($('#ftco-loader').length > 0) {
                $('#ftco-loader').removeClass('show');
            }
        }, 1);
    };
    loader();

    // Scrollax
    //$.Scrollax();

    // scroll
    //var scrollWindow = function () {
    //    $(window).scroll(function () {
    //        var $w = $(this),
    //            st = $w.scrollTop(),
    //            navbar = $('.ftco_navbar'),
    //            sd = $('.js-scroll-wrap');

    //        if (st > 150) {
    //            if (!navbar.hasClass('scrolled')) {
    //                navbar.addClass('scrolled');
    //            }
    //        }
    //        if (st < 150) {
    //            if (navbar.hasClass('scrolled')) {
    //                navbar.removeClass('scrolled sleep');
    //            }
    //        }
    //        if (st > 350) {
    //            if (!navbar.hasClass('awake')) {
    //                navbar.addClass('awake');
    //            }

    //            if (sd.length > 0) {
    //                sd.addClass('sleep');
    //            }
    //        }
    //        if (st < 350) {
    //            if (navbar.hasClass('awake')) {
    //                navbar.removeClass('awake');
    //                navbar.addClass('sleep');
    //            }
    //            if (sd.length > 0) {
    //                sd.removeClass('sleep');
    //            }
    //        }
    //    });
    //};
    //scrollWindow();

    var contentWayPoint = function () {
        var i = 0;
        $('.ftco-animate').waypoint(function (direction) {

            if (direction === 'down' && !$(this.element).hasClass('ftco-animated')) {

                i++;

                $(this.element).addClass('item-animate');
                setTimeout(function () {

                    $('body .ftco-animate.item-animate').each(function (k) {
                        var el = $(this);
                        setTimeout(function () {
                            var effect = el.data('animate-effect');
                            if (effect === 'fadeIn') {
                                el.addClass('fadeIn ftco-animated');
                            } else if (effect === 'fadeInLeft') {
                                el.addClass('fadeInLeft ftco-animated');
                            } else if (effect === 'fadeInRight') {
                                el.addClass('fadeInRight ftco-animated');
                            } else {
                                el.addClass('fadeInUp ftco-animated');
                            }
                            el.removeClass('item-animate');
                        }, k * 50, 'easeInOutExpo');
                    });

                }, 100);

            }

        }, { offset: '95%' });
    };
    contentWayPoint();




    // magnific popup
    $('.image-popup').magnificPopup({
        type: 'image',
        closeOnContentClick: true,
        closeBtnInside: false,
        fixedContentPos: true,
        mainClass: 'mfp-no-margins mfp-with-zoom', // class to remove default margin from left and right side
        gallery: {
            enabled: true,
            navigateByImgClick: true,
            preload: [0, 1] // Will preload 0 - before current, and 1 after the current image
        },
        image: {
            verticalFit: true
        },
        zoom: {
            enabled: true,
            duration: 300 // don't foget to change the duration also in CSS
        }
    });

    $('.popup-youtube, .popup-vimeo, .popup-gmaps').magnificPopup({
        disableOn: 700,
        type: 'iframe',
        mainClass: 'mfp-fade',
        removalDelay: 160,
        preloader: false,

        fixedContentPos: false
    });



    var goHere = function () {

        $('.mouse-icon').on('click', function (event) {

            event.preventDefault();

            $('html,body').animate({
                scrollTop: $('.goto-here').offset().top
            }, 500, 'easeInOutExpo');

            return false;
        });
    };
    goHere();




})(jQuery);



function setValue(event, mult) {

    var idProd = $(event).attr('idProduct');
    var inputProd = document.getElementById("prod-" + idProd)
    var priceProd = document.getElementById("price-" + idProd)
    var descProd = document.getElementById("desc-" + idProd)

    var value = parseFloat(inputProd.value);

    if (mult === -1 && value === 0) {
        return;
    }

    if (inputProd.attributes.typeinput.value === "U") {
        value = value + (1 * mult);
    }
    else {
        value = value + (0.5 * mult);
    }
    opacityButtonArea(value, mult, idProd);

    inputProd.value = value

    let productFind = productos.find(item => item.idProducto === idProd);
    if (productFind) {
        productFind.peso = value;
        productFind.subTotal = value * productFind.precio;

        if (value === 0) {
            productos = productos.filter(item => item.idProducto != idProd);
        }
    }
    else {
        var prod = {
            idProducto: idProd,
            peso: value,
            precio: parseFloat(priceProd.attributes.precio.value),
            descripcion: descProd.attributes.descProd.value,
            subTotal: value * parseFloat(priceProd.attributes.precio.value),
            tipoVenta: inputProd.attributes.typeinput.value
        }

        productos.push(prod);
    }
    let total = 0;
    var textArea = document.getElementById("text-area-products");
    textArea.textContent = '';

    productos.forEach((a) => {
        textArea.innerText += `· ${a.descripcion}: $${Number.parseFloat(a.precio).toFixed(2)} x ${a.peso} = $ ${Number.parseFloat(a.subTotal).toFixed(2) } \n`;
        total += a.subTotal;
    });


    if (productos.length > 0) {
        $("#text-area-products").show();
        $("#btnTrash").show();
    }
    else {
        $("#text-area-products").hide();
        $("#btnTrash").hide();
    }

    document.getElementById("btnCompras").innerText = `Total: $ ${total}`;
}

function opacityButtonArea(value, mult, idProd) {

    if (mult === -1 && value === 0) {
        const element = document.querySelector('#bottom-area-' + idProd);
        element.style.removeProperty('opacity');
    }
    else if (mult === 1 && value > 0) {
        const element = document.querySelector('#bottom-area-' + idProd);
        element.style.opacity = 1;
    }
}

function clean() {
    document.getElementById("btnCompras").innerText = `Total: $0`;
    document.getElementById("text-area-products").textContent = '';
    $("#text-area-products").hide();
    $("#btnTrash").hide();
    productos = [];
}

function resumenVenta() {
    if (productos.length > 0) {
        let sum = 0;
        productos.forEach(value => {
            sum += value.subTotal;
        });

        var tableData = productos.map(value => {
            return (
                `<tr>
                       <td class="table-products" style="border-right-color: #ffffff00;"><span class="text-muted">$ ${Number.parseFloat(value.precio).toFixed(2) } x ${value.peso} ${value.tipoVenta}</span>. - ${value.descripcion}</td>
                       <td class="table-products" style="font-size: 12px; text-align: right;"><strong>$ ${Number.parseFloat(value.subTotal).toFixed(2) }</strong></td>
                    </tr>`
            );
        }).join('');

        tableData = tableData.concat(`<tr>
                       <td class="table-products" style="font-size: 14px; border-right-color: #ffffff00;"><strong>TOTAL</strong></td>
                       <td class="table-products" style="font-size: 14px; text-align: right;"><strong>$ ${ Number.parseFloat(sum).toFixed(2) }</strong></td>
                    </tr>`);

        const tableBody = document.querySelector("#tableProductos");
        tableBody.innerHTML = tableData;

        $("#modalData").modal("show")
    }
}