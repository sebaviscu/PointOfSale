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

    //$("#input-search").on("keydown", function search(e) {
    //    var text = document.getElementById("input-search").value;

    //    if (e.keyCode == 13 && text !== '') {
    //        SearchProductByText(text);
    //    }
    //});

    var inputElement = document.getElementById('input-search');

    inputElement.addEventListener('input', function () {
        var letraIngresada = inputElement.value.toLowerCase();

        var productElements = document.querySelectorAll('.product-list');

        productElements.forEach(function (productElement) {
            var descripcionProducto = productElement.getAttribute('description-prd').toLowerCase();

            if (descripcionProducto.includes(letraIngresada)) {
                productElement.style.display = 'block';
            } else {
                productElement.style.display = 'none';
            }
        });
    });


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

function searchToggle(obj, evt) {
    var container = $(obj).closest('.search-wrapper');
    var text = document.getElementById("input-search").value;

    if (!container.hasClass('active')) {
        container.addClass('active');
        evt.preventDefault();
    }
    else if (container.hasClass('active') && $(obj).closest('.input-holder').length == 0) {
        container.removeClass('active');
        container.find('.search-input').val('');
        if (text !== '') {
            $('.product-list').css('display', '');

            //$.ajax({
            //    url: window.location.origin + '/Shop/GetProductsByCategory?idCategoria=0',
            //    type: "get",
            //    success: function (result) {
            //        $("#dvCategoryResults").html(result);
            //        var elementosInput = document.querySelectorAll('.input-producto');

            //        elementosInput.forEach(function (elemento) {
            //            let idProd = elemento.id.replace("prod-", "");
            //            let productFind = productos.find(item => item.idProduct === idProd);

            //            if (productFind) {

            //                elemento.value = productFind.quantity;
            //                let elementArea = document.querySelector('#bottom-area-' + idProd);
            //                elementArea.style.opacity = 1;
            //            }
            //        });
            //    }
            //});
        }
    }
    else if (container.hasClass('active') && text !== '') {
        SearchProductByText(text);
    }
}

function SearchProductByText(text) {
    $.ajax({
        url: window.location.origin + '/Shop/GetProductsByDescription?text=' + text,
        type: "get",
        success: function (result) {
            $("#dvCategoryResults").html(result);
            var elementosInput = document.querySelectorAll('.input-producto');

            elementosInput.forEach(function (elemento) {
                let idProd = elemento.id.replace("prod-", "");
                let productFind = productos.find(item => item.idProduct === idProd);

                if (productFind) {

                    elemento.value = productFind.quantity;
                    let elementArea = document.querySelector('#bottom-area-' + idProd);
                    elementArea.style.opacity = 1;
                }
            });
        }
    });
}

function selectCategoria(event) {
    $('.product-list').css('display', '');

    var idCat = parseInt($(event).attr('cat-id'));

    if (idCat != 0) {
        $(`.product-list:not([category-prod="${idCat}"])`).css('display', 'none');
    }

    //$.ajax({
    //    url: window.location.origin + '/Shop/GetProductsByCategory?idCategoria=' + idCat,
    //    type: "get",
    //    success: function (result) {
    //        $("#dvCategoryResults").html(result);
    //        var elementosInput = document.querySelectorAll('.input-producto');

    //        elementosInput.forEach(function (elemento) {
    //            let idProd = elemento.id.replace("prod-", "");
    //            let productFind = productos.find(item => item.idProduct === idProd);

    //            if (productFind) {

    //                elemento.value = productFind.quantity;
    //                let elementArea = document.querySelector('#bottom-area-' + idProd);
    //                elementArea.style.opacity = 1;
    //            }
    //        });
    //    }
    //});
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
%0A· *Metodo de pago*: ${selectedText}
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
                sum += value.total;
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