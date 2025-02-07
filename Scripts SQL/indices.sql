CREATE INDEX IX_Product_IsActive ON Product (IsActive);

CREATE INDEX IX_ListaPrecio_Lista_IdProducto ON ListaPrecios (Lista, IdProducto);

CREATE INDEX IX_CodigoBarras_Codigo ON CodigoBarras (Codigo);

CREATE INDEX IX_Product_Description ON Product (Description);

