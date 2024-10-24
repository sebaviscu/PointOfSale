﻿let page = 1;
const pageSize = 12;
let isLoading = false;
let hasMoreProducts = true;
let debounceTimeout;
var productos = [];

$(window).scroll(function () {
    if ($(window).scrollTop() + $(window).height() >= $(document).height() - 50 && hasMoreProducts && !isLoading) {
        loadMoreProducts();
    }
});

$(document).ready(function () {

    loadMoreProducts();

    $(".btnCategoria").on("click", function () {
        page = 1;
        hasMoreProducts = true;
        $("#dvCategoryResults").empty();
        $('.btnCategoria').removeClass('active');
        $(this).addClass('active');
        loadMoreProducts();
    });

    $('#search-icon').on('click', function () {
        searchToggle($('.search-icon')[0]);
    });

    $('#input-search').on('input', debounce(function () {
        searchToggle($('.search-icon')[0]);
    }, 500));

    $("#btnCloseSearchText").on("click", function () {
        $("#input-search").val('');
        page = 1;
        hasMoreProducts = true;
        $("#dvCategoryResults").empty();
        loadMoreProducts();
    });

    $("#btnTrash").on("click", function () {
        clean();
    });

    $("#btnCompras").on("click", function () {
        resumenVenta();
    });

    $(".btnClose").on("click", function () {
        $("#modalData").modal("hide");
    });

    $("#btnFinalizar").on("click", function () {
        finalizarVenta();
    });

    otrasFunciones();
});

function loadMoreProducts() {
    if (isLoading || !hasMoreProducts) return;

    isLoading = true;
    $('#loader').show();

    const categoryId = $(".btnCategoria.active").attr("cat-id") || 0;
    const searchText = $("#input-search").val() || '';

    fetch(`/Shop/GetMoreProducts?page=${page}&pageSize=${pageSize}&categoryId=${categoryId}&searchText=${searchText}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok.');
            }
            return response.json();
        })
        .then(data => {
            if (!data.hasMoreProducts) {
                hasMoreProducts = false;
            }

            const pageClass = `-page-${page}`;
            let htmlWithPageClass = data.html.replace(/pluss-button/g, `pluss-button pluss${pageClass}`);
            htmlWithPageClass = htmlWithPageClass.replace(/less-button/g, `less-button less${pageClass}`);
            $("#dvCategoryResults").append(htmlWithPageClass);            $('#loader').hide();
            isLoading = false;
            page++;

            attachButtonEvents(pageClass); 

        })
        .catch(error => {
            console.error('Fetch error:', error);
            $('#loader').hide();
            isLoading = false;
        });
}

function searchToggle(obj) {
    var container = $(obj).closest('.search-wrapper');
    var text = document.getElementById("input-search").value;

    if (!container.hasClass('active')) {
        container.addClass('active');
        event.preventDefault();
    }
    else if (container.hasClass('active') && $(obj).closest('.input-holder').length == 0) {
        container.removeClass('active');
        container.find('.search-input').val('');
        if (text !== '') {
            $('.product-list').css('display', '');
        }
    }
    else if (container.hasClass('active') && text.length >= 3) {
        SearchProductByText(text);
    }
}

function SearchProductByText(text) {
    page = 1;
    hasMoreProducts = true;
    $("#dvCategoryResults").empty();

    fetch(`/Shop/GetMoreProducts?page=${page}&pageSize=${pageSize}&categoryId=${$(".btnCategoria.active").attr("cat-id") || 0}&searchText=${text}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok.');
            }
            return response.json();
        })
        .then(data => {
            if (!data.hasMoreProducts) {
                hasMoreProducts = false;
            }

            $("#dvCategoryResults").append(data.html);
            $('#loader').hide();
            isLoading = false;
            page++;

        })
        .catch(error => {
            console.error('Fetch error:', error);
            $('#loader').hide();
            isLoading = false;
        });
}

function debounce(func, wait) {
    return function () {
        const context = this;
        const args = arguments;
        clearTimeout(debounceTimeout);
        debounceTimeout = setTimeout(() => func.apply(context, args), wait);
    };
}

function attachButtonEvents(pageClass) {
    $(`.pluss${pageClass}`).on("click", function () {
        setValue(event.currentTarget, 1);
    });

    $(`.less${pageClass}`).on("click", function () {
        setValue(event.currentTarget, -1);
    });
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

    var elementosInput = document.querySelectorAll('.input-producto');

    elementosInput.forEach(function (elemento) {
        elemento.value = 0;
        let idProd = elemento.id.replace("prod-", "");
        let elementArea = document.querySelector('#bottom-area-' + idProd);
        elementArea.style.removeProperty('opacity');
    });
}

function resumenVenta() {
    if (productos.length > 0) {
        let sum = 0;
        productos.forEach(value => {
            sum += value.total;
        });

        var tableData = productos.map(value => {
            return (
                `<tr>
                       <td class="table-products" style="border-right-color: #ffffff00;"><span class="text-muted">$ ${Number.parseFloat(value.price).toFixed(2)} x ${value.quantity} ${value.tipoVenta}</span>. - ${value.DescriptionProduct}</td>
                       <td class="table-products" style="font-size: 12px; text-align: right;"><strong>$ ${Number.parseFloat(value.total).toFixed(2)}</strong></td>
                    </tr>`
            );
        }).join('');

        tableData = tableData.concat(`<tr>
                       <td class="table-products" style="font-size: 14px; border-right-color: #ffffff00;"><strong>TOTAL</strong></td>
                       <td class="table-products" style="font-size: 14px; text-align: right;"><strong>$ ${Number.parseFloat(sum).toFixed(2)}</strong></td>
                    </tr>`);

        const tableBody = document.querySelector("#tableProductos");
        tableBody.innerHTML = tableData;

        $("#modalData").modal("show")
    }

}


function finalizarVenta() {

    const inputs = $("input.input-validate").serializeArray();
    const inputs_without_value = inputs.filter((item) => item.value.trim() == "")

    if (inputs_without_value.length > 0) {
        const msg = `Debe completar los campos : "${inputs_without_value[0].name}"`;
        toastr.warning(msg);
        $(`input[name="${inputs_without_value[0].name}"]`).focus();
        return;
    }


    if (document.getElementById("cboFormaPago").value == '') {
        const msg = `Debe completaro el campo Forma de Pago`;
        toastr.warning(msg, "");
        return;
    }

    if (productos.length > 0) {

        var terminal = document.getElementById("cboFormaPago");
        var selectedText = terminal.options[terminal.selectedIndex].text;

        const model = structuredClone(BASIC_MODEL);
        model["nombre"] = $("#txtNombre").val();
        model["telefono"] = $("#txtTelefono").val();
        model["direccion"] = $("#txtDireccion").val();
        model["metodoPago"] = $("#cboFormaPago").val();
        model["comentario"] = $("#txtComentario").val();

        var inputPhone = document.getElementById("txtPhone");
        var phone = inputPhone.attributes.phoneNumber.value;

        let sum = 0;
        productos.forEach(value => {
            sum += value.total;
        });

        var textWA = `*NUEVO PEDIDO*%0A`;

        textWA = textWA.concat(`
%0A· *Nombre*: ${model.nombre}
%0A· *Telefono*: ${model.telefono} 
%0A· *Direccion*: ${model.direccion} 
%0A· *Forma de pago*: ${selectedText}
%0A· *Comentarios*: ${model.comentario} 
%0A· *TOTAL*: $${Number.parseFloat(sum).toFixed(2)}%0A`);

        textWA += productos.map(value => {
            return (
                `%0A - _${value.DescriptionProduct}_: ${value.quantity} ${value.tipoVenta}`
            );
        }).join('');


        $("#modalData").modal("hide")

        swal({
            title: "MUCHAS GRACIAS!",
            text: `Se abrirá un chat de Whatsapp con nosotros, para que nos envies el pedido.`,
            type: "success"
        }, function (value) {

            // Whatsapp
            window.open('https://wa.me/' + phone + '?text=' + textWA, '_blank');

            productos.forEach(value => {
                delete value.tipoVenta;
            });

            const sale = {
                idFormaDePago: model.metodoPago,
                total: sum,
                detailSales: productos,
                nombre: model.nombre,
                direccion: model.direccion,
                telefono: model.telefono,
                comentario: model.comentario,
                estado: 0
            }

            fetch("/Shop/RegisterSale", {
                method: "POST",
                headers: { 'Content-Type': 'application/json;charset=utf-8' },
                body: JSON.stringify(sale)
            }).then(response => {
                return response.ok ? response.json() : Promise.reject(response);
            }).then(responseJson => {

                if (responseJson.state) {
                }
            })

            clean();
        });
    }
}

const BASIC_MODEL = {
    nombre: '',
    telefono: '',
    direccion: '',
    metodoPago: '',
    comentario: '',
    total: ''
}


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

    let productFind = productos.find(item => item.idProduct === idProd);
    if (productFind) {
        productFind.quantity = value;
        productFind.total = value * productFind.price;

        if (value === 0) {
            productos = productos.filter(item => item.idProduct != idProd);
        }
    }
    else {
        var prod = {
            idProduct: idProd,
            quantity: value,
            price: parseFloat(priceProd.attributes.precio.value),
            DescriptionProduct: descProd.attributes.descProd.value,
            total: value * parseFloat(priceProd.attributes.precio.value),
            tipoVenta: inputProd.attributes.typeinput.value
        }

        productos.push(prod);
    }
    let total = 0;
    var textArea = document.getElementById("text-area-products");
    textArea.textContent = '';

    let productsToDisplay = productos.slice(-5);
    productos.forEach((a) => {
        total += a.total;
    });

    productsToDisplay.forEach((a) => {
        textArea.innerText += `· ${a.DescriptionProduct}: $${Number.parseFloat(a.price).toFixed(2)} x ${a.quantity} = $ ${Number.parseFloat(a.total).toFixed(2)} \n`;
    });

    $("#btnTrash").text(productos.length).append('<i class="mdi mdi-trash-can mdi-18px"></i>');

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

function otrasFunciones() {

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
}

AOS.init({
    duration: 800,
    easing: 'slide'
});