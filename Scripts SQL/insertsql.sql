go

insert into Tienda (nombre) values('Mercado Don Pepe')
go

insert into rol([description],isActive) values
('Admin',1),
('Empleado',1),
('Encargado',1)

go

insert into Users(name,email,phone,idRol,[password],photo,isActive) values
('admin','admin','909090',1,'123',null,1)

go

insert into Menu([description],icon,isActive, orden, controller, pageAction) values
('Configuracion','mdi mdi-settings',1,4,null,null),
('Inventario','mdi mdi-package-variant-closed',1,3,null,null),
('Venta','mdi mdi-shopping',1,7,'Sales','NewSale'),
('Reportes','mdi mdi-chart-bar',1,5,null,null),
('Dashboard','mdi mdi-view-dashboard-outline',1,1,'Admin','DashBoard'),
('Admin','mdi mdi-account',1,2,null,null),
('Web','mdi mdi-web',1,6,null,null) --7

go

--*menu child - Admin
insert into Menu([description],idMenuParent,controller,pageAction,isActive) values
('Users',6,'Admin','Users',1), --8
('Tiendas',1, 'Tienda', 'Tienda', 1), --9
('Categorias',1,'Inventory','Categories',1), --10
('Productos',2,'Inventory','Products',1), --11
('Sales history',4,'Sales','SalesHistory',1), --12
('Sales report',4,'Reports','SalesReport',1), --13
('Formas de Pago',1,'Admin','TipoVenta',1), --14
('Clientes',6,'Admin','Cliente',1), --15
('Proveedor',6,'Admin','Proveedor',1), --16
('Promociones',2,'Admin','Promociones',1), --17
('Gastos',6,'Gastos','Gastos',1), --18
('Turno',6,'Turno','Turno',1), --19
('Ventas Web',7,'Shop','VentaWeb',1), --20
('Shop',7,'Shop','Index',1) --21
go

UPDATE Menu SET idMenuParent = idMenu where idMenuParent is null

update Menu set idMenuParent=null where idMenu=3
update Menu set idMenuParent=null where idMenu=5

go
--________________________________ INSERT MENU ROLE ________________________________


--*Admin
INSERT INTO RolMenu(idRol,idMenu,isActive) values
(1,3,1),
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
(1,21,1)

--*empleado
INSERT INTO RolMenu(idRol,idMenu,isActive) values
(2,3,1),
(2,7,1),
(2,20,1),
(2,21,1)


--*encargado
INSERT INTO RolMenu(idRol,idMenu,isActive) values
(3,2,1),
(3,3,1),
(3,4,1),
(3,10,1),
(3,11,1),
(3,17,1),
(3,7,1),
(3,20,1),
(3,21,1)
go
--________________________________ INSERT CATEGORIES ________________________________

INSERT INTO Category([description],isActive) values
('Frutas',1),
('Verduras',1),
('Congelados',1),
('Almacen',1)

go

--________________________________ INSERT TYPEDOCUMENTSALE ________________________________

insert into TypeDocumentSale([description],isActive, tipoFactura) values
('Efectivo',1,0),
('Tarjeta de Debito',1,0),
('Tarjeta de Credito',1,0),
('Mercado Pago',1,0),
('Billetera Santa Fe',1,0)

go
--________________________________ INSERT CORRELATIVE NUMBER ________________________________

--000001
insert into CorrelativeNumber(lastNumber,quantityDigits,management,dateUpdate) values
(0,6,'Sale',getdate())

