function applyPromociones(totalQuantity, data, currentTab) {
    
    if (data.idproduct) {
        data = {
            id: data.idproduct,
            text: data.descriptionproduct,
            categoryProducty: data.categoryProducty,
            category: data.category || null,
            iva: data.iva,
            tipoVenta: data.tipoVenta,
            modificarPrecio: data.modificarPrecio,
            precioAlMomento: data.precioAlMomento,
            price: parseFloat(data.price),
            excluirPromociones: data.excluirPromociones,
            promocion: data.promocion
        };
    }

    let currentdate = new Date();
    let today = currentdate.getDay().toString();

    let promPorDia = promociones.find(item => item.dias.includes(today) && !item.idProducto && item.idCategory.length === 0);
    let promPorCat = promociones.find(item => item.idCategory.includes(data.category) && !item.idProducto && (!item.dias.length || item.dias.includes(today)));
    let promPorProducto = promociones.find(item => parseInt(item.idProducto) === data.id);

    data.promocion = '';

    if (promPorProducto) {
        data = applyForProduct(promPorProducto, totalQuantity, data, currentTab);
    } else if (promPorCat) {
        data = calcularPrecioPorcentaje(data, promPorCat, totalQuantity);
        data.promocion = promPorCat.nombre;
    } else if (promPorDia) {
        data = calcularPrecioPorcentaje(data, promPorDia, totalQuantity);
        data.promocion = promPorDia.nombre;
    }

    return data;
}

function applyForProduct(prom, totalQuantity, data, currentTab) {
    let apply = false;

    if (prom.operador === null && prom.precio !== null) {
        data = calcularPrecioPorcentaje(data, prom, totalQuantity);
        apply = true;
    } else if (prom.operador !== null) {
        switch (prom.operador) {
            case 0:
                if (totalQuantity < prom.cantidadProducto) return data;

                let diffDividido = totalQuantity % prom.cantidadProducto;

                if (diffDividido === 0) {
                    data = calcularPrecioPorcentaje(data, prom, totalQuantity);
                    apply = true;
                } else if (diffDividido > 0) {
                    let precio = parseFloat(data.price);
                    if (isNaN(precio)) return data;

                    let newProd = new Producto();
                    newProd.idproduct = data.id;
                    newProd.descriptionproduct = data.text;
                    newProd.categoryProducty = data.categoryProducty;
                    newProd.iva = data.iva;
                    newProd.quantity = diffDividido;
                    newProd.price = precio.toFixed(2).toString();
                    newProd.total = (precio * diffDividido).toFixed(2).toString();

                    currentTab.products.push(newProd);

                    let difCant = totalQuantity - diffDividido;
                    data = calcularPrecioPorcentaje(data, prom, difCant);
                    apply = true;
                }
                break;

            case 1:
                if (totalQuantity >= prom.cantidadProducto) {
                    data = calcularPrecioPorcentaje(data, prom, totalQuantity);
                    apply = true;
                }
                break;
        }
    }

    if (apply) data.promocion = prom.nombre;

    return data;
}

function calcularPrecioPorcentaje(data, prom, totalQuantity) {
    let precio = prom.precio !== null ? prom.precio : parseFloat(data.price) * (1 - (prom.porcentaje / 100));

    data.diferenciapromocion = (parseFloat(data.price) * totalQuantity) - (precio * totalQuantity);
    data.total = precio * totalQuantity;
    data.price = precio;
    data.quantity = totalQuantity;

    return data;
}