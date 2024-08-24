go
insert into Tienda (nombre, idListaPrecio) values('Mercado Don Pepe', 1)
go

insert into rol([description],isActive) values
('Admin',1),
('Empleado',1),
('Encargado',1)

go

insert into Users(name,email,idRol,[password],isActive) values
('admin','admin',1,'',1)

go

insert into Menu([description],icon,isActive, orden, controller, pageAction) values
('Configuracion','mdi mdi-settings',1,4,null,null), --1
('Inventario','mdi mdi-package-variant-closed',1,3,null,null), --2
('Venta','mdi mdi-shopping',1,7,'Sales','NewSale'), --3
('Reportes','mdi mdi-chart-bar',1,5,null,null), --4
('Dashboard','mdi mdi-view-dashboard-outline',1,1,'Admin','DashBoard'), --5
('Admin','mdi mdi-account',1,2,null,null), --6
('Web','mdi mdi-web',1,6,null,null) --7

go

--*menu child - Admin
insert into Menu([description],idMenuParent,controller,pageAction,isActive) values
('Usuarios',6,'Admin','Users',1), --8
('Puntos de venta',1, 'Tienda', 'Tienda', 1), --9
('Categorias',1,'Inventory','Categories',1), --10
('Productos',2,'Inventory','Products',1), --11
('Reporte Ventas',4,'Sales','SalesHistory',1), --12
('Formas de Pago',1,'Admin','TipoVenta',1), --13
('Clientes',6,'Admin','Cliente',1), --14
('Proveedores',6,'Admin','Proveedor',1), --15
('Promociones',2,'Admin','Promociones',1), --16
('Gastos',6,'Gastos','Gastos',1), --17
('Turnos',6,'Turno','Turno',1), --18
('Ventas Web',7,'Shop','VentaWeb',1), --19
('Shop',7,'Shop','Index',1), --20
('Reporte Productos',4,'Reports','ProductsReport',1), --21
('Notificaciones',6,'Notification','Notification',1), --22
('Pedidos',2,'Pedido','Pedido',1), --23
('Ajustes',1,'Admin','Ajuste',1), --24
('Libro de IVA',4,'Reports','LibroIva',1), -- 25
('Stock',2,'Inventory','Stock',1), -- 26
('Facturacion',6,'Admin','Facturacion',1) -- 27

go

UPDATE Menu SET idMenuParent = idMenu where idMenuParent is null

update Menu set idMenuParent=null where idMenu=3
update Menu set idMenuParent=null where idMenu=5

go
--________________________________ INSERT MENU ROLE ________________________________


--*Admin
INSERT INTO RolMenu(idRol,idMenu,isActive) values
(1,1,1),
(1,2,1),
(1,3,1),
(1,4,1),
(1,5,1),
(1,6,1),
(1,7,1),
(1,8,1),
(1,9,1),
(1,10,1),
(1,11,1),
(1,12,1),
(1,13,1),
(1,14,1),
(1,15,1),
(1,16,1),
(1,17,1),
(1,1,1),
(1,18,1),
(1,19,1),
(1,20,1),
(1,21,1),
(1,22,1),
(1,23,1),
(1,24,1),
(1,25,1),
(1,26,1),
(1,27,1)

--*empleado
INSERT INTO RolMenu(idRol,idMenu,isActive) values
(2,3,1),
(2,7,1),
(2,19,1),
(2,20,1)

--*encargado
INSERT INTO RolMenu(idRol,idMenu,isActive) values
(3,2,1),
(3,3,1),
(3,4,1),
(3,11,1),
(3,12,1),
(3,7,1),
(3,19,1),
(3,20,1),
(3,21,1),
(3,16,1),
(3,26,1)

go
--________________________________ INSERT CATEGORIES ________________________________

INSERT INTO Category([description],isActive) values
('Frutas',1),
('Congelados',1),
('Almacen',1)

go

--________________________________ INSERT TYPEDOCUMENTSALE ________________________________

insert into TypeDocumentSale([description],isActive, tipoFactura, web,comision) values
('Efectivo',1,0,1,0),
('Tarjeta de Debito',1,0,1,0),
('Tarjeta de Credito',1,0,1,0),
('Mercado Pago',1,0,1,0),
('Transferencia',1,0,1,0)

go
--________________________________ INSERT Ajustes ________________________________

DECLARE @NuevoIdAjuste INT;

INSERT INTO Ajustes (MinimoIdentificarConsumidor, IdTienda)
VALUES (500000, 1);

SET @NuevoIdAjuste = SCOPE_IDENTITY();

UPDATE Tienda 
SET idAjustes = @NuevoIdAjuste

go

DECLARE @NuevoIdAjusteFacturacion INT;

INSERT INTO AjustesFacturacion (IdTienda)
VALUES (1);

SET @NuevoIdAjusteFacturacion = SCOPE_IDENTITY();

UPDATE Tienda 
SET idAjustesFacturacion = @NuevoIdAjusteFacturacion
WHERE IdTienda = 1; 

GO


go

insert into AjustesWeb(MontoEnvioGratis,AumentoWeb,Whatsapp,Lunes,Martes,Miercoles,Jueves,Viernes,Sabado,Domingo,Feriado,Facebook,Instagram,Tiktok,
Twitter,Youtube, direccion, telefono,nombre) 
values (0,0,'','','','','','','','','','','','','','','','','Mercado Don Pepe')

go
--________________________________ INSERT CORRELATIVE NUMBER ________________________________

--000001
DECLARE @NuevoIdCorrelativeNumber INT;

INSERT INTO CorrelativeNumber (lastNumber, quantityDigits, management, idTienda, dateUpdate)
VALUES (0, 6, 'Sale', 1, GETDATE());

SET @NuevoIdCorrelativeNumber = SCOPE_IDENTITY();

UPDATE Tienda 
SET idCorrelativeNumber = @NuevoIdCorrelativeNumber

GO

INSERT INTO CorrelativeNumber (lastNumber, quantityDigits, management, dateUpdate)
VALUES (0, 6, 'SaleWeb', GETDATE());

GO