go
insert into Tienda (nombre, idListaPrecio, color) values('Mercado', 1, '#4c84ff')
go

insert into rol([description],isActive) values
('Admin',1),
('Empleado',1),
('Encargado',1)

go

insert into Users(name,email,idRol,[password],isActive, sinHorario) values
('admin','admin',1,'',1, 1)

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
('Productos',2,'Inventory','Products',1), --10
('Reporte Ventas',4,'Sales','SalesHistory',1), --11
('Clientes',6,'Admin','Cliente',1), --12
('Proveedores',6,'Proveedores','Index',1), --13
('Promociones',2,'Inventory','Promociones',1), --14
('Gastos',6,'Gastos','Gastos',1), --15
('Turnos',6,'Turno','Turno',1), --16
('Ventas Web',7,'Shop','VentaWeb',1), --17
('Shop',7,'Shop','Index',1), --18
('Reporte Productos',4,'Reports','ProductsReport',1), --19
('Notificaciones',6,'Notification','Notification',1), --20
('Pedidos',2,'Pedido','Pedido',1), --21
('Ajustes',1,'Ajustes','Index',1), --22
('Libro de IVA',4,'Reports','LibroIva',1), -- 23
('Stock',2,'Inventory','Stock',1), -- 24
('Facturacion',6,'Facturacion','Index',1), -- 25
('Movimientos de Caja',6,'MovimientoCaja','Index',1), -- 26
('Tablas',1,'Tablas','Index',1), -- 27
('Reporte Precios',4,'Reports','PreciosReport',1) --28

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
(1,27,1),
(1,28,1)


--*empleado
INSERT INTO RolMenu(idRol,idMenu,isActive) values
(2,3,1),
(2,7,1),
(2,11,1),
(2,17,1),
(2,18,1),
(2,28,1)

--*encargado
INSERT INTO RolMenu(idRol,idMenu,isActive) values
(3,2,1),
(3,3,1),
(3,4,1),
(3,7,1),
(3,10,1),
(3,11,1),
(3,14,1),
(3,17,1),
(3,18,1),
(3,24,1),
(3,28,1)

go
--________________________________ INSERT CATEGORIES ________________________________

INSERT INTO Category([description],isActive) values
('Frutas',1),
('Congelados',1),
('Almacen',1)

go

--________________________________ INSERT TYPEDOCUMENTSALE ________________________________

insert into TypeDocumentSale([description],isActive, tipoFactura, web,comision) values
('Efectivo',1,3,1,0),
('Tarjeta de Debito',1,1,1,0),
('Tarjeta de Credito',1,1,1,0),
('Mercado Pago',1,1,1,0),
('Modo',1,1,1,0),
('Transferencia',1,1,1,0)

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

INSERT INTO AjustesFacturacion (IdTienda, IsProdEnvironment)
VALUES (1,0);

SET @NuevoIdAjusteFacturacion = SCOPE_IDENTITY();

UPDATE Tienda 
SET idAjustesFacturacion = @NuevoIdAjusteFacturacion
WHERE IdTienda = 1; 

GO


insert into AjustesWeb(MontoEnvioGratis,AumentoWeb,Whatsapp,Lunes,Martes,Miercoles,Jueves,Viernes,Sabado,Domingo,Feriado,Facebook,Instagram,Tiktok,
Twitter,Youtube, direccion, telefono,nombre) 
values (0,0,'','','','','','','','','','','','','','','','','Mercado')

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

INSERT INTO CorrelativeNumber (lastNumber, quantityDigits, management,idTienda,  dateUpdate)
VALUES (0, 6, 'SaleWeb',1, GETDATE());

GO

insert into TipoGastos (gastoPArticular, descripcion, iva) values 
(0, 'Sueldos', 0)

go

insert into FormatosVenta (formato, valor, estado) values 
('Unidad', 1, 1),
('100 gr', 100,1),
('200 gr', 200,1),
('250 gr', 250,1),
('500 gr', 500,1),
('750 gr', 750,1),
('1 kg', 1000,1)


