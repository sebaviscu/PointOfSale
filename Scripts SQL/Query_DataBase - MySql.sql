

create table Tienda(
idTienda int primary key AUTO_INCREMENT,
nombre varchar(150) null,
Telefono varchar(50) null,
Direccion varchar(100) null,
idListaPrecio int null,
Logo LONGBLOB null,
Color VARCHAR(7) NOT NULL,
modificationDate datetime null,
modificationUser varchar(50) null
)

go

CREATE TABLE Turno (
idTurno INT PRIMARY KEY AUTO_INCREMENT,
fechaInicio DATETIME NOT NULL,
fechaFin DATETIME NULL,
descripcion VARCHAR(150) NULL,
idTienda INT NOT NULL,
registrationDate DATETIME NOT NULL,
registrationUser VARCHAR(50) NOT NULL,
modificationUser VARCHAR(50) NULL,
ObservacionesApertura varchar(200) null,
ObservacionesCierre varchar(200) null,
TotalInicioCaja decimal(10,2) not null,
TotalCierreCajaSistema decimal(10,2) null,
TotalCierreCajaReal decimal(10,2) null,
ErroresCierreCaja LONGTEXT  NULL,
ValidacionRealizada TINYINT(1) null,
BilletesEfectivo LONGTEXT  NULL,
FOREIGN KEY (idTienda) REFERENCES Tienda(idTienda) ON DELETE CASCADE
);


create table Menu(
idMenu int primary key AUTO_INCREMENT,
description varchar(30),
idMenuParent int,
icon varchar(30),
controller varchar(30),
pageAction varchar(30),
isActive TINYINT(1),
orden int null,
registrationDate DATETIME NULL,
FOREIGN KEY (idMenu) references Menu(idMenu)
);

create table Rol(
idRol int primary key AUTO_INCREMENT,
description varchar(30),
isActive TINYINT(1),
registrationDate DATETIME NULL
);
 
 create table RolMenu(
 idRolMenu int primary key AUTO_INCREMENT,
 idRol int references Rol(idRol),
 idMenu int references Menu(idMenu),
 isActive TINYINT(1),
 registrationDate DATETIME NULL,
FOREIGN KEY (idRol) references Rol(idRol),
FOREIGN KEY (idMenu) references Menu(idMenu)
 );
 
create table Users(
idUsers int primary key AUTO_INCREMENT,
name varchar(50),
email varchar(50),
phone varchar(50),
idRol int,
password varchar(100),
photo LONGBLOB,
isActive TINYINT(1),
sinHorario TINYINT(1) null,
IsSuperAdmin  TINYINT(1) null,
registrationDate datetime null,
modificationDate datetime null,
modificationUser varchar(50) null,
idTienda int null,
FOREIGN KEY (idTienda) references Tienda(idTienda),
FOREIGN KEY (idRol) references Rol(idRol)
);


create table Category(
idCategory int primary key AUTO_INCREMENT,
description varchar(50),
isActive TINYINT(1),
registrationDate DATETIME NULL,
modificationDate datetime null,
modificationUser varchar(50) null
);

create table Proveedor(
	idProveedor int primary key AUTO_INCREMENT,
	nombre varchar(150) not null,
	cuil varchar(50) null,
	telefono varchar(50) null,
	direccion varchar(200) null,
	NombreContacto varchar(100) null,
	Telefono2 varchar(30) null,
	Telefono3 varchar(30) null,
	Email varchar(100) null,
	web varchar(200) null,
	comentario varchar(200) null,
	iva decimal(10,2) null,
	tipoFactura int null,
	registrationDate datetime not null,
	modificationDate datetime null,
	modificationUser varchar(50) null
);

CREATE TABLE FormatosVenta (
    id INT PRIMARY KEY AUTO_INCREMENT,
    formato VARCHAR(50),
    valor FLOAT,
	estado TINYINT(1) not null
);

create table Product(
idProduct int primary key AUTO_INCREMENT,
description varchar(100),
price decimal(10,2),
photo LONGBLOB,
isActive TINYINT(1),
priceWeb decimal(10,2) null,
precioFormatoWeb decimal(10,2) null,
formatoWeb int null,
porcentajeProfit int null,
costPrice decimal(10,2) null,
tipoVenta int not null,
comentario varchar(300)  null,
iva decimal(10,2) null,
destacado TINYINT(1) null,
productoWeb TINYINT(1) null,
modificarPrecio TINYINT(1) null,
PrecioAlMomento TINYINT(1) null,
ExcluirPromociones TINYINT(1) null,
idCategory int null,
idProveedor int null,
sku varchar(50) null,
IncluirIvaEnPrecio TINYINT(1) null,
modificationDate datetime null,
modificationUser varchar(50) null,
registrationDate datetime null,
FOREIGN KEY (idProveedor) REFERENCES Proveedor(idProveedor),
FOREIGN KEY (idCategory) REFERENCES Category(idCategory)
);

create table CorrelativeNumber(
idCorrelativeNumber int primary key AUTO_INCREMENT,
lastNumber int,
quantityDigits int,
management varchar(100),
idTienda INT null,
dateUpdate datetime,
FOREIGN KEY (idTienda) REFERENCES Tienda(idTienda) ON DELETE CASCADE
);

create table TypeDocumentSale(
idTypeDocumentSale int primary key AUTO_INCREMENT,
description varchar(50),
isActive TINYINT(1),
web TINYINT(1) null,
tipoFactura int not null,
comision decimal(10,2) not null,
DescuentoRecargo INT null,
registrationDate DATETIME NULL
);

create table Sale(
idSale int primary key AUTO_INCREMENT,
saleNumber varchar(6),
idTypeDocumentSale int null,
customerDocument varchar(10),
clientName varchar(20),
total decimal(10,2),
idTurno int not null,
IdTienda int not null,
idClienteMovimiento int null,
descuentoRecargo decimal(10,2) null,
IdFacturaEmitida int null,
isWeb TINYINT(1) null,
isDelete TINYINT(1) not null,
Observaciones LONGTEXT ,
ResultadoFacturacion TINYINT(1) null,
tipoFactura int null,
registrationDate DATETIME NULL,
registrationUser varchar(50) not null,
FOREIGN KEY (idTypeDocumentSale) references TypeDocumentSale(idTypeDocumentSale),
FOREIGN KEY (idTurno) references Turno(idTurno)
);

create table VentaWeb(
idVentaWeb int primary key AUTO_INCREMENT,
saleNumber varchar(6),
Nombre varchar(100) null,
Telefono varchar(50) null,
Direccion varchar(100) null,
Comentario varchar(200) null,
idFormaDePago int null,
Total decimal(10,2) not null,
IdTienda int null,
estado int not null,
isEdit TINYINT(1) null,
editText LONGTEXT  null,
descuentoRetiroLocal decimal(10,2) null,
cruceCallesDireccion LONGTEXT  null,
CostoEnvio decimal(10,2) null,
ObservacionesUsuario LONGTEXT  null,
tipoFactura int null,
idSale int null,
registrationDate datetime not null,
modificationDate datetime null,
modificationUser varchar(50) null,
FOREIGN KEY (idFormaDePago) references TypeDocumentSale(idTypeDocumentSale),
FOREIGN KEY (idSale) references Sale(idSale)
);

create table DetailSale(
idDetailSale int primary key AUTO_INCREMENT,
idSale int,
idProduct int,
brandProduct varchar(100),
descriptionProduct varchar(100),
categoryProducty varchar(100),
quantity decimal(10,2),
promocion varchar(300) null,
price decimal(10,2),
total decimal(10,2),
tipoVenta int null,
iva decimal(10,2) null,
idVentaWeb int null,
Recogido TINYINT(1) null,
FOREIGN KEY (idSale) references Sale(idSale),
FOREIGN KEY (idVentaWeb) references VentaWeb(idVentaWeb)
);

create table Cliente(
	idCliente int primary key AUTO_INCREMENT,
	nombre varchar(150) not null,
	cuil varchar(50) null,
	telefono varchar(50) null,
	direccion varchar(200) null,
	IdTienda int not null,
	isActive TINYINT(1),
	CondicionIva int null,
	Comentario varchar(200) null ,
	IdFacturaEmitida int null,
	registrationDate datetime not null,
	modificationDate datetime null,
	modificationUser varchar(50) null,
FOREIGN KEY (IdTienda) references Tienda(idTienda)
);

create table ClienteMovimiento(
	idClienteMovimiento int primary key AUTO_INCREMENT,
	idCliente int not null,
	idSale int null,
	total decimal(10,2) not null,
	IdTienda int not null,
	TipoMovimiento int not null,
	registrationDate datetime not null,
	registrationUser varchar(50) not null,
FOREIGN KEY (idCliente) references Cliente(idCliente),
FOREIGN KEY (idSale) references Sale(idSale),
FOREIGN KEY (IdTienda) references Tienda(idTienda)
);

create table Promocion(
	idPromocion int primary key AUTO_INCREMENT,
	nombre varchar(100) null,
	idProducto varchar(100) null,
	operador int null,
	cantidadProducto int null,
	idCategory varchar(100) null,
	dias varchar(100) null,
	precio decimal(10,2) null,
	porcentaje decimal(10,2) null,
	IdTienda int not null,
	isActive TINYINT(1),
	registrationDate datetime not null,
	modificationDate datetime null,
	modificationUser varchar(50) null,
FOREIGN KEY (IdTienda) references Tienda(idTienda)
);


create table ProveedorMovimiento(
	idProveedorMovimiento int primary key AUTO_INCREMENT,
	idProveedor int not null,
	importe decimal(10,2) not null,
	importeSinIva decimal(10,2) null,
	Iva decimal(10,2) null,
	Ivaimporte decimal(10,2) null,
	nroFactura varchar(50) null,
	tipoFactura varchar(50) null,
	comentario varchar(300)  null,
	idTienda int not null,
	EstadoPago int not null,
	FacturaPendiente TINYINT(1) not null,
	FormaPago int null,
	modificationDate datetime null,
	modificationUser varchar(50) null,
	registrationDate datetime not null,
	registrationUser varchar(50) not null,
FOREIGN KEY (idProveedor) references Proveedor(idProveedor)
);

create table TipoGastos(
	idTipoGastos int primary key AUTO_INCREMENT,
	gastoParticular int not null,
	descripcion varchar(150) not null,
	iva decimal(10,2) null,
	tipoFactura int null
);

create table Gastos(
	idGastos int primary key AUTO_INCREMENT,
	idTipoGasto int not null,
	importe decimal(10,2) not null,
	importeSinIva decimal(10,2) null,
	Iva decimal(10,2),
	Ivaimporte decimal(10,2) null,
	nroFactura varchar(50) null,
	tipoFactura varchar(50) null,
	comentario varchar(300) null,
	idTienda int not null,
	EstadoPago int not null,
	FacturaPendiente TINYINT(1) not null,
	GastoAsignado varchar(150) null,
	registrationDate datetime not null,
	registrationUser varchar(50) not null,
	modificationDate datetime null,
	modificationUser varchar(50) null,
FOREIGN KEY (idTipoGasto) references TipoGastos(idTipoGastos)
);

create table AuditoriaModificaciones(
idAuditoriaModificaciones int primary key AUTO_INCREMENT,
entidad varchar(50) not null,
idEntidad int not null,
descripcion varchar(150) not null,
entidadAntes LONGTEXT  not null,
entidadDespues LONGTEXT  not null,
modificationDate datetime null,
modificationUser varchar(50) null
);

create table Notifications(
idNotifications int primary key AUTO_INCREMENT,
descripcion LONGTEXT  not null,
isActive TINYINT(1) not null,
accion varchar(100) null,
rols varchar(20) null,
idUser int null,
idTienda int null,
registrationUser varchar(50),
registrationDate datetime not null,
modificationDate datetime null,
modificationUser varchar(50) null
);

create table ListaPrecios(
idListaPrecios int primary key AUTO_INCREMENT,
lista int not null,
idProducto int not null,
precio decimal(10,2) not null,
porcentajeProfit int not null,
registrationDate DATETIME NULL,
FOREIGN KEY (idProducto) references Product(idProduct)
);

create table Vencimientos(
idVencimiento int primary key AUTO_INCREMENT,
lote varchar(100) null,
fechaVencimiento datetime not null,
fechaElaboracion datetime null,
notificar TINYINT(1) not null,
idProducto int  not null,
idTienda int not null,
registrationDate DATETIME NULL,
registrationUser varchar(50),
FOREIGN KEY (idProducto) references Product(idProduct),
FOREIGN KEY (idTienda) references Tienda(idTienda)
);

create table Pedidos(
idPedido int primary key AUTO_INCREMENT,
importeEstimado decimal(10,2) null,
estado int not null,
comentario varchar(200) null,
fechaRecibido datetime null,
idProveedorMovimiento int  null,
idProveedor int,
idTienda int null,
fechaCerrado datetime  null,
usuarioFechaCerrado varchar(100)  null,
importeFinal decimal(10,2)  null,
registrationDate DATETIME NULL,
registrationUser varchar(50) not null,
FOREIGN KEY (idTienda) references Tienda(idTienda),
FOREIGN KEY (idProveedor) references Proveedor(idProveedor),
FOREIGN KEY (idProveedorMovimiento) references ProveedorMovimiento(idProveedorMovimiento)
);

create table PedidoProducto(
IdPedidoProducto int primary key AUTO_INCREMENT,
cantidadProducto int not null,
lote varchar(100) null,
vencimiento datetime null,
cantidadProductoRecibida int  null,
idProducto int not null,
idPedido int null,
FOREIGN KEY (idProducto) references Product(idProduct),
FOREIGN KEY (idPedido) references Pedidos(idPedido)
);

create table AjustesWeb(
idAjusteWeb int primary key AUTO_INCREMENT,
email VARCHAR(200) NULL,
MontoEnvioGratis decimal(10,2) null,
AumentoWeb decimal(10,2) null,
CostoEnvio decimal(10,2) null,
CompraMinima decimal(10,2) null,
TakeAwayDescuento decimal(10,2) null,
HabilitarTakeAway TINYINT(1) NULL,
Whatsapp varchar(50) null,
Facebook varchar(50) null,
Instagram varchar(50) null,
Tiktok varchar(50) null,
Twitter varchar(50) null,
Youtube varchar(80) null,
direccion varchar(50) null,
telefono varchar(50) null,
nombre varchar(50) null,
habilitarWeb TINYINT(1) NULL,
NombreComodin1 varchar(150) null,
HabilitarComodin1 TINYINT(1) NULL,
NombreComodin2 varchar(150) null,
HabilitarComodin2 TINYINT(1) NULL,
NombreComodin3 varchar(150) null,
HabilitarComodin3 TINYINT(1) NULL,
IvaEnPrecio TINYINT(1) NULL,
SobreNosotros LONGTEXT  NULL,
TemrinosCondiciones LONGTEXT  NULL,
PoliticaPrivacidad LONGTEXT  NULL,
logoImagenNombre VARCHAR(50) NULL,
modificationDate datetime null,
modificationUser varchar(50) null
);

CREATE TABLE Ajustes (
idAjuste INT PRIMARY KEY AUTO_INCREMENT,
codigoSeguridad VARCHAR(20) NULL,
ImprimirDefault TINYINT(1) NULL,
Encabezado1 VARCHAR(100) NULL,
Encabezado2 VARCHAR(100) NULL,
Encabezado3 VARCHAR(100) NULL,
Pie1 VARCHAR(100) NULL,
Pie2 VARCHAR(200) NULL,
Pie3 VARCHAR(200) NULL,
NombreImpresora VARCHAR(500) NULL,
MinimoIdentificarConsumidor BIGINT NULL,
ControlStock TINYINT(1) NULL,
FacturaElectronica TINYINT(1) NULL,
idTienda INT NOT NULL,
ControlEmpleado TINYINT(1) NULL,
NotificarEmailCierreTurno TINYINT(1) NULL,
ControlTotalesCierreTurno TINYINT(1) NULL,
EmailEmisorCierreTurno VARCHAR(50) NULL,
PasswordEmailEmisorCierreTurno LONGTEXT  NULL,
EmailsReceptoresCierreTurno VARCHAR(50) NULL,
modificationDate DATETIME NULL,
modificationUser VARCHAR(50) NULL,
FOREIGN KEY (idTienda) references Tienda(idTienda) ON DELETE CASCADE
);


CREATE TABLE AjustesFacturacion (
idAjustesFacturacion INT PRIMARY KEY AUTO_INCREMENT,
cuit BIGINT NULL,
PuntoVenta INT NULL, 
CondicionIva INT NULL,
CertificadoPassword VARCHAR(250) NULL,
CertificadoNombre VARCHAR(150) NULL,
CertificadoFechaInicio DATETIME NULL,
CertificadoFechaCaducidad DATETIME NULL,
IngresosBurutosNro VARCHAR(50) NULL,
DireccionFacturacion VARCHAR(200) NULL,
FechaInicioActividad DATETIME NULL,
NombreTitular VARCHAR(70) NULL,
idTienda INT NOT NULL,
IsProdEnvironment TINYINT(1) not null,
modificationDate DATETIME NULL,
modificationUser VARCHAR(50) NULL,
FOREIGN KEY (idTienda) references Tienda(idTienda) ON DELETE CASCADE
);

ALTER TABLE Tienda 
ADD idAjustes INT NULL,
ADD CONSTRAINT FK_Tienda_Ajustes FOREIGN KEY (idAjustes) REFERENCES Ajustes(idAjuste);

ALTER TABLE Tienda 
ADD IdAjustesFacturacion INT NULL,
ADD CONSTRAINT FK_Tienda_AjustesFacturacion FOREIGN KEY (IdAjustesFacturacion) REFERENCES AjustesFacturacion(IdAjustesFacturacion);

ALTER TABLE Tienda 
ADD IdCorrelativeNumber INT NULL,
ADD CONSTRAINT FK_Tienda_CorrelativeNumber FOREIGN KEY (IdCorrelativeNumber) REFERENCES CorrelativeNumber(IdCorrelativeNumber);


create table Stock(
idStock int primary key AUTO_INCREMENT,
StockActual decimal(10,2) not null,
StockMinimo int not null,
idProducto int not null,
idTienda int not null,
FOREIGN KEY (idProducto) references Product(idProduct) ON DELETE CASCADE,
FOREIGN KEY (idTienda) references Tienda(idTienda) ON DELETE CASCADE
);

CREATE TABLE FacturasEmitidas (
    IdFacturaEmitida INT AUTO_INCREMENT PRIMARY KEY,
    CAE VARCHAR(150) null,
    CAEVencimiento DATETIME null,
    FechaEmicion DATETIME,
    NroDocumento BIGINT,
    TipoDocumentoId INT,
    TipoDocumento VARCHAR(50),
	PuntoVenta INT not null,
	NroFactura INT null,
	TipoFactura VARCHAR(50) not null,
    Resultado VARCHAR(50),
    Observaciones LONGTEXT , 
	ImporteTotal decimal(10,2) not null,
	ImporteNeto decimal(10,2) not null,
	ImporteIva decimal(10,2) not null,
	IdFacturaAnulada int null,
    FacturaAnulada varchar(50) null,
	idSale int not null,
	idCliente int null,
	IdTienda int  not null,
    RegistrationUser VARCHAR(200),
    RegistrationDate DATETIME,
FOREIGN KEY (idSale) references Sale(idSale),
FOREIGN KEY (idTienda) references Cliente(idCliente),
FOREIGN KEY (idCliente) references Tienda(idTienda)
);

CREATE TABLE CodigoBarras (
    IdCodigoBarras INT AUTO_INCREMENT PRIMARY KEY,
    Codigo VARCHAR(50) not null,
    Descripcion VARCHAR(50) null,
	idProducto int not null,
FOREIGN KEY (idProducto) references Product(idProduct) ON DELETE CASCADE
);


CREATE TABLE RazonMovimientoCaja (
    IdRazonMovimientoCaja INT PRIMARY KEY AUTO_INCREMENT,
    Descripcion VARCHAR(255) NOT NULL,
    Tipo INT NOT NULL,
	estado TINYINT(1) not null
);

CREATE TABLE MovimientoCaja (
    IdMovimientoCaja INT PRIMARY KEY AUTO_INCREMENT,
    Comentario VARCHAR(500) NULL,
    RegistrationDate DATETIME NOT NULL,
    RegistrationUser VARCHAR(50) NOT NULL,
	importe decimal(10,2),
    IdRazonMovimientoCaja INT NOT NULL,
	idTienda int not null,
	idTurno int not null,
FOREIGN KEY (IdRazonMovimientoCaja) references RazonMovimientoCaja(IdRazonMovimientoCaja),
FOREIGN KEY (idTurno) references Turno(idTurno)
);

CREATE TABLE Tags (
    IdTag INT PRIMARY KEY AUTO_INCREMENT,
    Nombre VARCHAR(20) NOT NULL,
    Color VARCHAR(7) NOT NULL
);


CREATE TABLE ProductTags (
    ProductId INT NOT NULL,
    TagId INT NOT NULL,
    PRIMARY KEY (ProductId, TagId),
FOREIGN KEY (ProductId) references Product(idProduct) ON DELETE CASCADE,
FOREIGN KEY (TagId) references Tags(IdTag) ON DELETE CASCADE
);

CREATE TABLE Lov (
    Id INT PRIMARY KEY AUTO_INCREMENT,
	descripcion varchar(50) NOT NULL,
	estado TINYINT(1) NOT NULL,
    LovType INT NOT NULL,
	registrationDate datetime null,
	RegistrationUser varchar(150),
	modificationDate datetime null,
	modificationUser varchar(50) null
);

CREATE TABLE Horario (
    Id INT PRIMARY KEY AUTO_INCREMENT,  
    horaEntrada VARCHAR(5) NOT NULL,         
    horaSalida VARCHAR(5) NOT NULL,          
    DiaSemana INT NOT NULL,                   
    IdUsuario INT NOT NULL,                   
    RegistrationDate DATETIME, 
	RegistrationUser varchar(150),
    ModificationDate DATETIME,                
    ModificationUser varchar(150),           
	FOREIGN KEY (IdUsuario) REFERENCES Users(idUsers) ON DELETE CASCADE
);

CREATE TABLE Empresa (
    Id INT PRIMARY KEY AUTO_INCREMENT,  
    RazonSocial VARCHAR(150) NOT NULL,
    NombreContacto VARCHAR(150) NULL,
    NumeroContacto VARCHAR(50) NULL,
    Licencia INT NOT NULL,
    ProximoPago DATETIME NULL, 
    FrecuenciaPago INT NULL, 
    Comentario varchar(300) null, 
    RegistrationDate DATETIME,     
	RegistrationUser varchar(150),
    ModificationDate DATETIME,                
    ModificationUser varchar(150)
);

CREATE TABLE PagoEmpresa (
    Id INT PRIMARY KEY AUTO_INCREMENT,  
    FechaPago DATETIME NOT NULL, 
    Importe decimal(10,2) NOT NULL,
	Comentario varchar(300) NULL,
	EstadoPago int NULL,
	importeSinIva decimal(10,2) null,
	Iva decimal(10,2) null,
	Ivaimporte decimal(10,2) null,
	nroFactura varchar(50) null,
	tipoFactura int null,
	FacturaPendiente TINYINT(1) null,
	IdEmpresa INT NOT NULL,
    RegistrationDate DATETIME,     
	RegistrationUser varchar(150),
    ModificationDate DATETIME,                
    ModificationUser varchar(150),
	FOREIGN KEY (IdEmpresa) REFERENCES Empresa(Id) ON DELETE CASCADE
);

CREATE TABLE ProductLov
(
    ProductId INT NOT NULL,
    LovId INT NOT NULL,
    LovType INT NOT NULL,

    PRIMARY KEY (ProductId, LovId, LovType),

    FOREIGN KEY (ProductId) 
        REFERENCES Product(IdProduct) ON DELETE CASCADE,

    FOREIGN KEY (LovId) 
        REFERENCES Lov(Id) ON DELETE CASCADE
);

create table BackupProducto(
Id int primary key AUTO_INCREMENT, 
CorrelativeNumberMasivo VARCHAR(10) null,
RegistrationUser varchar(50) null,
RegistrationDate datetime null,
idProduct int,
description varchar(100),
price decimal(10,2),
photo LONGBLOB,
isActive TINYINT(1),
priceWeb decimal(10,2) null,
precioFormatoWeb decimal(10,2) null,
formatoWeb int null,
porcentajeProfit int null,
costPrice decimal(10,2) null,
tipoVenta int not null,
comentario varchar(300) null,
iva decimal(10,2) null,
destacado TINYINT(1) null,
productoWeb TINYINT(1) null,
modificarPrecio TINYINT(1) null,
PrecioAlMomento TINYINT(1) null,
ExcluirPromociones TINYINT(1) null,
Category varchar(100) null,
Proveedor varchar(100) null,
Precio1 decimal(10,2) null,
PorcentajeProfit1 int null,
Precio2 decimal(10,2) null,
PorcentajeProfit2 int null,
Precio3 decimal(10,2) null,
PorcentajeProfit3 int null,
sku varchar(50) null
);

CREATE TABLE VentasPorTipoDeVentaTurno (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Descripcion LONGTEXT  NULL,
    TotalSistema DECIMAL(18, 2) NULL,
    TotalUsuario DECIMAL(18, 2) NULL,
    Error LONGTEXT  NULL,
    IdTurno INT NOT NULL,
FOREIGN KEY (IdTurno) REFERENCES Turno(IdTurno)
);


CREATE TABLE HistorialLogin (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Informacion LONGTEXT  NOT NULL,
    Fecha DATETIME NOT NULL,
    IdUser INT NOT NULL,
    UserName VARCHAR(255) NOT NULL,
    FOREIGN KEY (IdUser) REFERENCES Users(idUsers) ON DELETE CASCADE
);


CREATE TABLE HorariosWeb (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    DiaSemana int NOT NULL,
    HoraInicio TIME NOT NULL,
    HoraFin TIME NOT NULL,
	idAjusteWeb INT NOT NULL,
    FOREIGN KEY (idAjusteWeb) REFERENCES AjustesWeb(idAjusteWeb) ON DELETE CASCADE
);
