AOS.init({
    duration: 800,
    easing: 'slide'
});

var productos = [];

(function ($) {


    fetch("/Admin/GetTipoVentaWeb")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {
            $("#cboFormaPago").append(
                $("<option>").val('').text('')
            )
            if (responseJson.data.length > 0) {
                responseJson.data.forEach((item) => {
                    $("#cboFormaPago").append(
                        $("<option>").val(item.idTypeDocumentSale).text(item.description)
                    )
                });
            }
        })

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

    $("#btnFinalizar").on("click", function () {
        finalizarVenta();
    })

    $(".btnCategoria").on("click", function () {
        selectCategoria(event.currentTarget);
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

})(jQuery);


function selectCategoria(event) {

    var idCat = $(event).attr('cat-id');
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

    productos.forEach((a) => {
        textArea.innerText += `· ${a.DescriptionProduct}: $${Number.parseFloat(a.price).toFixed(2)} x ${a.quantity} = $ ${Number.parseFloat(a.total).toFixed(2)} \n`;
        total += a.total;
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
        toastr.warning(msg, "");
        $(`input[name="${inputs_without_value[0].name}"]`).focus();
        return;
    }


    if (document.getElementById("cboFormaPago").value == '') {
        const msg = `Debe completaro el campo Forma de Pago`;
        toastr.warning(msg, "");
        return;
    }

    if (productos.length > 0) {

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

        var text = productos.map(value => {
            return (
                ` - ${value.DescriptionProduct}: ${value.quantity} ${value.tipoVenta}\n`
            );
        }).join('');

        text = text.concat(`\n
                · Nombre: ${model.nombre} \n` +
            `· Telefono: ${model.telefono} \n` +
            `· Direccion: ${model.direccion} \n` +
            `· Metodo de pago: ${model.metodoPago} \n` +
            `· Comentario: ${model.comentario} \n\n` +
            `· TOTAL: ${Number.parseFloat(sum).toFixed(2)}`);


        $("#modalData").modal("hide")

        //window.open('https://wa.me/' + phone + '?text=' + text, '_blank');


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

                swal("Registrado!", `MUCHAS GRACIAS!!`, "success");
                clean();
            }
        }).catch((error) => {
        })
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