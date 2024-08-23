
-- Crear un índice sobre la columna Description para mejorar las consultas LIKE
CREATE INDEX IDX_Product_Description ON Product(Description);

-- Crear un índice sobre la columna BarCode para mejorar las consultas que la utilizan
CREATE INDEX IDX_Product_BarCode ON Product(BarCode);

-- Crear un índice sobre la columna IsActive
CREATE INDEX IDX_Product_IsActive ON Product(IsActive);

-- Crear un índice sobre IdProduct para mejorar el rendimiento en los filtros de esta columna
CREATE INDEX IDX_Product_IdProduct ON Product(IdProduct);

-- Crear un índice compuesto sobre IdProducto y Lista en la tabla ListaPrecios
CREATE INDEX IDX_ListaPrecio_IdProducto_Lista ON ListaPrecios(IdProducto, Lista);

-- Crear un índice sobre la columna RegistrationDate para mejorar el filtrado por rango de fechas
CREATE INDEX IDX_Sale_RegistrationDate ON Sale(RegistrationDate);

-- Crear un índice sobre la columna SaleNumber para mejorar las consultas que utilizan EndsWith
CREATE INDEX IDX_Sale_SaleNumber ON Sale(SaleNumber);

-- Crear un índice sobre la columna IdSale para mejorar la ordenación por IdSale
CREATE INDEX IDX_Sale_IdSale ON Sale(IdSale);

-- Crear un índice sobre la columna RegistrationDate para mejorar el filtrado por fecha
CREATE INDEX IDX_ProveedorMovimiento_RegistrationDate ON ProveedorMovimiento(RegistrationDate);

-- Crear un índice sobre la columna EstadoPago para mejorar las consultas que la utilizan
CREATE INDEX IDX_ProveedorMovimiento_EstadoPago ON ProveedorMovimiento(EstadoPago);

-- Crear un índice sobre la columna idTienda para mejorar el filtrado por tienda
CREATE INDEX IDX_ProveedorMovimiento_idTienda ON ProveedorMovimiento(idTienda);

-- Crear un índice Nombre para mejorar el rendimiento en las agrupaciones
CREATE INDEX IDX_Proveedor_Nombre ON proveedor( Nombre);

-- Crear un índice sobre la columna IdClienteMovimiento para mejorar el filtrado
CREATE INDEX IDX_Sale_IdClienteMovimiento ON Sale(IdClienteMovimiento);

-- Crear un índice sobre la columna IdTienda para mejorar el filtrado por tienda
CREATE INDEX IDX_Sale_IdTienda ON Sale(IdTienda);

-- Crear un índice compuesto sobre Description y TipoFactura en la tabla TypeDocumentSale
CREATE INDEX IDX_TypeDocumentSale_Description_TipoFactura ON TypeDocumentSale(Description, TipoFactura);

-- Crear un índice sobre la columna CategoryProducty para mejorar el filtrado por categoría
CREATE INDEX IDX_DetailSale_CategoryProducty ON DetailSale(CategoryProducty);

-- Crear un índice sobre la columna DescriptionProduct para mejorar el rendimiento en las agrupaciones
CREATE INDEX IDX_DetailSale_DescriptionProduct ON DetailSale(DescriptionProduct);

-- Crear un índice sobre la columna RegistrationDate para mejorar el filtrado por rango de fechas
CREATE INDEX IDX_Gastos_RegistrationDate ON Gastos(RegistrationDate);

-- Crear un índice sobre la columna GastoParticular para mejorar las consultas que la utilizan
CREATE INDEX IDX_TipoDeGasto_GastoParticular ON TipoGastos(GastoParticular);

-- Crear un índice sobre la columna EstadoPago para mejorar el filtrado por estado de pago
CREATE INDEX IDX_Gastos_EstadoPago ON Gastos(EstadoPago);

-- Crear un índice sobre la columna IdTienda para mejorar el filtrado por tienda
CREATE INDEX IDX_Gastos_IdTienda ON Gastos(IdTienda);

-- Crear un índice sobre la columna Descripcion para mejorar el rendimiento en las agrupaciones
CREATE INDEX IDX_TipoDeGasto_Descripcion ON TipoGastos(Descripcion);

