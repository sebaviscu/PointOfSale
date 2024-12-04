let page = 1;
const pageSize = 12;
let isLoading = false;
let hasMoreProducts = true;
let debounceTimeout;
let productos = [];
let primerCargaCache = true;
let montoEnvioGratis = 0;
let ajustesWeb;
let descuentoTakeAway = 0;

const BASIC_MODEL_SHOP = {
    nombre: '',
    telefono: '',
    direccion: '',
    metodoPago: '',
    comentario: '',
    total: '',
    cruceCallesDireccion: '',
    costoEnvio: 0
}

$(window).scroll(function () {
    if ($(window).scrollTop() + $(window).height() >= $(document).height() - 50 && hasMoreProducts && !isLoading) {
        loadMoreProducts();
    }
});
//ajustarDivTextPrice();

$(document).ready(function () {

    loadMoreProducts();
    montoEnvioGratis = $('#txtMontoEnvioGratis').text().trim() != null ? parseFloat($('#txtMontoEnvioGratis').text().trim()) : null;

    fetch("/Ajustes/GetAjustesWeb")
        .then(response => {

            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {
                ajustesWeb = responseJson.object;

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        })

    //setTimeout(function () {

    //    productos = JSON.parse(localStorage.getItem('productos')) || [];
    //    productos.forEach(function (p) {

    //        opacityButtonArea(p.quantity, 1, p.idProduct);
    //        document.getElementById("prod-" + p.idProduct).value = p.quantity;
    //    });
    //    dibujarAreaTotales();
    //}, 5000);

    var elem = document.getElementById('switchTakeAway');
    var init = new Switchery(elem, { color: '#1ab394', size: 'small' });

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

//$(window).on('resize', function () {
//    ajustarDivTextPrice();
//});

//function ajustarDivTextPrice() {
//    setTimeout(function () {
//        if ($(window).width() < 700) {
//            $('.div-text-price').addClass('d-flex');
//            $('.div-text-price').addClass('justify-content-around');
//            $('.div-text-price').addClass('mt-2');
//        } else {
//            $('.div-text-price').removeClass('d-flex');
//            $('.div-text-price').removeClass('justify-content-around');
//            $('.div-text-price').removeClass('mt-2');
//        }
//    }, 700);
//}

function fetchProducts(page, pageSize, categoryId, tagId, searchText) {
    isLoading = true;
    $('#loader').show();

    const productsQuantity = Object.fromEntries(
        productos.map(product => [product.idProduct, product.quantity])
    );

    fetch(`/Shop/GetMoreProducts?page=${page}&pageSize=${pageSize}&categoryId=${categoryId}&searchText=${searchText}&tagId=${tagId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(productsQuantity),
    })
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
            $("#dvCategoryResults").append(htmlWithPageClass);
            $('#loader').hide();
            isLoading = false;
            page++;

            attachButtonEvents(pageClass);

            if (primerCargaCache) {
                productos = JSON.parse(localStorage.getItem('productos')) || [];
                productos.forEach(function (p) {

                    opacityButtonArea(p.quantity, 1, p.idProduct);
                    document.getElementById("prod-" + p.idProduct).value = p.quantity;
                });
                dibujarAreaTotales();
                primerCargaCache = false;
            }
        })
        .catch(error => {
            console.error('Fetch error:', error);
            $('#loader').hide();
            isLoading = false;
        });
}

// Función para cargar más productos
function loadMoreProducts() {
    if (isLoading || !hasMoreProducts) return;

    let categoryId = $(".btnCategoria.active").attr("cat-id") || 0;
    let tagId = $(".btnCategoria.active").attr("tag-id");
    let searchText = $("#input-search").val() || '';

    // Reutilizar la función fetchProducts
    fetchProducts(page, pageSize, categoryId, tagId, searchText);
}

// Función para buscar productos por texto
function SearchProductByText(text) {
    page = 1;
    hasMoreProducts = true;
    $("#dvCategoryResults").empty();

    let categoryId = $(".btnCategoria.active").attr("cat-id") || 0;
    let tagId = $(".btnCategoria.active").attr("tag-id");
    let searchText = $("#input-search").val() || '';

    fetchProducts(page, pageSize, categoryId, tagId, searchText);
}
function searchToggle(obj) {
    let container = $(obj).closest('.search-wrapper');
    let text = document.getElementById("input-search").value;

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

    let elementosInput = document.querySelectorAll('.input-producto');

    elementosInput.forEach(function (elemento) {
        elemento.value = 0;
        let idProd = elemento.id.replace("prod-", "");
        let elementArea = document.querySelector('#bottom-area-' + idProd);
        elementArea.style.removeProperty('opacity');
    });
    localStorage.removeItem('productos');

}

function resumenVenta() {
    if (productos.length > 0) {
        let sum = 0;
        productos.forEach(value => {
            sum += value.total;
        });

        let tableDataShoop = productos.map(value => {
            return (
                `<tr>
                       <td class="table-products" style="border-right-color: #ffffff00;"><span class="text-muted">${value.descriptionProduct} - $ ${parseFloat(value.price).toFixed(0)} x ${value.quantity} ${value.tipoVenta}</span>.</td>
                       <td class="table-products" style="font-size: 12px; text-align: right;"><strong>$ ${parseFloat(value.total).toFixed(0)}</strong></td>
                    </tr>`
            );
        }).join('');

        let total = parseFloat(sum);

        descuentoTakeAway = 0;
        if (ajustesWeb.habilitarTakeAway) {

            descuentoTakeAway = parseFloat(total * (parseInt(ajustesWeb.takeAwayDescuento) / 100));

            tableDataShoop = tableDataShoop.concat(`<tr>
                       <td class="table-products td-discount" style="font-size: 14px; border-right-color: #ffffff00; display: none;"><strong>Descuento ${parseInt(ajustesWeb.takeAwayDescuento)}% por retiro en el local</strong ></td >
                       <td class="table-products td-discount" style="font-size: 14px; text-align: right; display: none;"><strong>- $ ${descuentoTakeAway.toFixed(0)}</strong></td>
                    </tr>`);
        }

        const divMensajeEnvio = document.getElementById("divMensajeEnvio");
        let totalSinTakeAway = total;

        if (total < ajustesWeb.compraMinima) {
            divMensajeEnvio.className = "alert alert-warning d-flex align-items-center";
            document.getElementById("alertTitle").textContent = `Compra mínima $${ajustesWeb.compraMinima}`;
            $('#btnFinalizar').prop('disabled', true);
        }
        else {
            $('#btnFinalizar').prop('disabled', false);

            if (total >= montoEnvioGratis) {
                divMensajeEnvio.className = "alert alert-success d-flex align-items-center";
                document.getElementById("alertTitle").textContent = "¡¡ Envio Gratis !!";
            } else if (total < montoEnvioGratis) {
                divMensajeEnvio.className = "alert alert-warning d-flex align-items-center";
                document.getElementById("alertTitle").textContent = `Solo faltan $${montoEnvioGratis - total.toFixed(0)} para que el Envio sea GRATIS !!`;

                tableDataShoop = tableDataShoop.concat(`<tr>
                       <td class="table-products td-envio" style="font-size: 13px; border-right-color: #ffffff00;"><strong>Envio</strong></td>
                       <td class="table-products td-envio" style="font-size: 13px; text-align: right;"><strong>$ ${ajustesWeb.costoEnvio}</strong></td>
                    </tr>`);
                totalSinTakeAway = total + parseFloat(ajustesWeb.costoEnvio);
            }
        }

        tableDataShoop = tableDataShoop.concat(`<tr>
                       <td class="table-products" style="font-size: 18px; border-right-color: #ffffff00;"><strong>TOTAL</strong></td>
                       <td class="table-products" id="td-sin-descuento" style="font-size: 18px; text-align: right;"><strong>$ ${totalSinTakeAway.toFixed(0)}</strong></td>
                       <td class="table-products td-discount" style="font-size: 18px; text-align: right; display: none;"><strong>$ ${(total - descuentoTakeAway).toFixed(0)}</strong></td>
                    </tr>`);

        const tableBody = document.querySelector("#tableProductos");
        tableBody.innerHTML = tableDataShoop;

        $("#modalData").modal("show");

        document.getElementById("switchTakeAway").checked = false;
    }

}

$('#switchTakeAway').change(function () {
    let check = document.getElementById("switchTakeAway").checked;

    $('#txtDireccion').prop('disabled', check);
    if (check)
        $('#txtDireccion').val('');

    let discountElements = document.getElementsByClassName("td-discount");

    for (let element of discountElements) {
        element.style.display = check ? '' : 'none';
    }

    discountElements = document.getElementsByClassName("td-envio");

    for (let element of discountElements) {
        element.style.display = check ? 'none' : '';
    }

    document.getElementById("td-sin-descuento").style.display = check ? 'none' : '';
});

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

        let terminal = document.getElementById("cboFormaPago");
        let selectedText = terminal.options[terminal.selectedIndex].text;

        let model = structuredClone(BASIC_MODEL_SHOP);
        model["nombre"] = $("#txtNombre").val();
        model["telefono"] = $("#txtTelefono").val();
        model["direccion"] = $("#txtDireccion").val();
        model["metodoPago"] = $("#cboFormaPago").val();
        model["comentario"] = $("#txtComentario").val();
        model["cruceCallesDireccion"] = $("#txtCruceCallesDireccion").val();


        let inputPhone = document.getElementById("txtPhone");
        let phone = inputPhone.attributes.phoneNumber.value;

        let sum = 0;
        productos.forEach(value => {
            sum += value.total;
        });

        let envio = "$" + ajustesWeb.costoEnvio;
        let totalWS = sum;
        let checkTakeAway = document.getElementById("switchTakeAway").checked;

        if (checkTakeAway) {
            totalWS -=  descuentoTakeAway;
            envio = "Retiro en persona";
        }
        else {
            if (totalWS < montoEnvioGratis) {
                totalWS += ajustesWeb.costoEnvio;
                model["costoEnvio"] = ajustesWeb.costoEnvio;
            }
            else {
                envio = "GRATIS";
            }
        }

        let textWA = `*NUEVO PEDIDO*%0A`;

        textWA = textWA.concat(`
%0A· *Nombre*: ${model.nombre}
%0A· *Telefono*: ${model.telefono} 
%0A· *Direccion*: ${model.direccion} 
%0A· *Forma de pago*: ${selectedText}
%0A· *Comentarios*: ${model.comentario} 
%0A· *Envio*: ${envio} 
%0A· *TOTAL*: $${totalWS.toFixed(2)}%0A`);

        textWA += productos.map(value => {
            return (
                `%0A - _${value.descriptionProduct}_: ${value.quantity} ${value.tipoVenta}`
            );
        }).join('');

        localStorage.removeItem('productos');

        $("#modalData").modal("hide")

        swal({
            title: "MUCHAS GRACIAS!",
            text: `Se abrirá un chat de Whatsapp con nosotros, para que nos envies el pedido.`,
            type: "success"
        }, function (value) {

            // Whatsapp
            //window.open('https://wa.me/' + phone + '?text=' + textWA, '_blank');

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
                estado: 0,
                descuentoRetiroLocal: checkTakeAway ? descuentoTakeAway : 0,
                cruceCallesDireccion: model.cruceCallesDireccion,
                costoEnvio: model.costoEnvio
            }

            fetch("/Shop/RegisterSale", {
                method: "POST",
                headers: { 'Content-Type': 'application/json;charset=utf-8' },
                body: JSON.stringify(sale)
            }).then(response => {
                return response.json();
            }).then(responseJson => {

                if (!responseJson.state) {
                    swal("Lo sentimos", responseJson.message, "error");
                }
            })

            clean();
        });
    }
}



function setValue(event, mult) {

    let idProd = $(event).attr('idProduct');
    let inputProd = document.getElementById("prod-" + idProd)
    let priceProd = document.getElementById("price-" + idProd)
    let descProd = document.getElementById("desc-" + idProd)

    let value = parseFloat(inputProd.value);
    let formatoWeb = 1;

    let price = parseFloat(priceProd.attributes.precio.value);
    if (mult === -1 && value === 0) {
        return;
    }

    if (inputProd.attributes.typeinput.value === "U") {
        value = value + (1 * mult);
    }
    else {

        formatoWeb = parseInt(inputProd.attributes.formatoWeb.value);
        formatoWeb = formatoWeb / 1000;
        value = value + (formatoWeb * mult);
        price = price * (1 / formatoWeb);
    }

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
        let prod = {
            idProduct: idProd,
            quantity: value,
            price: price,
            descriptionProduct: descProd.attributes.descProd.value,
            total: value * price,
            tipoVenta: inputProd.attributes.typeinput.value,
            formatoWeb: formatoWeb
        }

        productos.push(prod);
    }

    opacityButtonArea(value, mult, idProd);
    dibujarAreaTotales();

    // chache
    localStorage.setItem('productos', JSON.stringify(productos));
}

function dibujarAreaTotales() {

    let total = 0;
    let textArea = document.getElementById("text-area-products");
    textArea.textContent = '';

    let productsToDisplay = productos.slice(-5);
    productos.forEach((a) => {
        total += a.total;
    });

    productsToDisplay.forEach((a) => {
        textArea.innerHTML += `· ${a.descriptionProduct}: $${Number.parseFloat(a.price).toFixed(0)} x ${a.quantity} ${a.tipoVenta} = $ ${Number.parseFloat(a.total).toFixed(0)}<br>`;
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

    if (total >= montoEnvioGratis && !pasoEnvioGratis) {
        toastr.success("", "¡¡ Envio gratis !!");
        pasoEnvioGratis = true;
    }
    else if (total < montoEnvioGratis)
        pasoEnvioGratis = false;

    document.getElementById("btnCompras").innerText = `Total: $ ${total}`;
}

let pasoEnvioGratis = false;
function otrasFunciones() {

    "use strict";

    let isMobile = {
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


    let fullHeight = function () {

        $('.js-fullheight').css('height', $(window).height());
        $(window).resize(function () {
            $('.js-fullheight').css('height', $(window).height());
        });

    };
    fullHeight();

    // loader
    let loader = function () {
        setTimeout(function () {
            if ($('#ftco-loader').length > 0) {
                $('#ftco-loader').removeClass('show');
            }
        }, 1);
    };
    loader();

    let contentWayPoint = function () {
        let i = 0;
        $('.ftco-animate').waypoint(function (direction) {

            if (direction === 'down' && !$(this.element).hasClass('ftco-animated')) {

                i++;

                $(this.element).addClass('item-animate');
                setTimeout(function () {

                    $('body .ftco-animate.item-animate').each(function (k) {
                        let el = $(this);
                        setTimeout(function () {
                            let effect = el.data('animate-effect');
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

    let goHere = function () {

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