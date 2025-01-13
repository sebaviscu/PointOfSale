

--create database POINTOFSALE

create table Tienda(
idTienda int primary key identity(1,1),
nombre varchar(150) null,
Telefono varchar(50) null,
Direccion varchar(100) null,
idListaPrecio int null,
Logo varbinary(max) null,
Color NVARCHAR(7) NOT NULL,
[modificationDate] [datetime] null,
[modificationUser] varchar(50) null,
)

go

CREATE TABLE Turno (
idTurno INT PRIMARY KEY IDENTITY(1,1),
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
ErroresCierreCaja VARCHAR(max) NULL,
ValidacionRealizada bit null,
BilletesEfectivo NVARCHAR(max) NULL,
CONSTRAINT FK_Turno_Tienda FOREIGN KEY (idTienda)
REFERENCES Tienda(idTienda) ON DELETE CASCADE
);

go

create table Menu(
[idMenu] int primary key identity(1,1),
[description] varchar(30),
[idMenuParent] int references Menu(idMenu),
[icon] varchar(30),
[controller] varchar(30),
[pageAction] varchar(30),
[isActive] bit,
[orden] int null,
[registrationDate] datetime default getdate()
)

go


create table Rol(
[idRol] int primary key identity(1,1),
[description] varchar(30),
[isActive] bit,
[registrationDate] datetime default getdate()
)

 go
 
 create table RolMenu(
 [idRolMenu] int primary key identity(1,1),
 [idRol] int references Rol(idRol),
 [idMenu] int references Menu(idMenu),
 [isActive] bit,
 [registrationDate] datetime default getdate()
 )

 go


create table Users(
[idUsers] int primary key identity(1,1),
[name] varchar(50),
[email] varchar(50),
[phone] varchar(50),
[idRol] int references Rol(idRol),
[password] varchar(100),
[photo] varbinary(max),
[isActive] bit,
sinHorario bit null,
IsSuperAdmin  bit null,
[registrationDate] datetime default getdate(),
[modificationDate] [datetime] null,
[modificationUser] varchar(50) null,
idTienda int references Tienda(idTienda) null
)

go

create table Category(
[idCategory] int primary key identity(1,1),
[description] varchar(50),
[isActive] bit,
[registrationDate] datetime default getdate(),
[modificationDate] [datetime] null,
[modificationUser] varchar(50) null
)
go

create table Proveedor(
	[idProveedor] int primary key identity(1,1),
	[nombre] varchar(150) not null,
	[cuil] varchar(50) null,
	[telefono] varchar(50) null,
	[direccion] varchar(200) null,
	NombreContacto varchar(100) null,
	Telefono2 varchar(30) null,
	Telefono3 varchar(30) null,
	Email varchar(100) null,
	web varchar(200) null,
	comentario varchar(200) null,
	iva decimal(10,2) null,
	tipoFactura int null,
	[registrationDate] datetime not null,
	[modificationDate] datetime null,
	[modificationUser] varchar(50) null
)

go

CREATE TABLE FormatosVenta (
    id INT PRIMARY KEY IDENTITY(1,1),
    formato VARCHAR(50),
    valor FLOAT,
	estado BIT not null
);

go 
create table Product(
[idProduct] int primary key identity(1,1),
[description] varchar(100),
[price] decimal(10,2),
[photo] varbinary(max),
[isActive] bit,
priceWeb decimal(10,2) null,
precioFormatoWeb decimal(10,2) null,
formatoWeb int null,
porcentajeProfit int null,
costPrice decimal(10,2) null,
tipoVenta int not null,
[comentario] varchar(300)  null,
iva decimal(10,2) null,
destacado bit null,
productoWeb bit null,
modificarPrecio bit null,
PrecioAlMomento bit null,
ExcluirPromociones bit null,
[idCategory] int references Category(idCategory),
[idProveedor] int references Proveedor(idProveedor) null,
sku varchar(50) null,
[modificationDate] datetime null,
[modificationUser] varchar(50) null,
[registrationDate] datetime null
)

go

create table CorrelativeNumber(
[idCorrelativeNumber] int primary key identity(1,1),
[lastNumber] int,
[quantityDigits] int,
[management] varchar(100),
idTienda INT null,
[dateUpdate] datetime
CONSTRAINT FK_CorrelativeNumber_Tienda FOREIGN KEY (idTienda)
    REFERENCES Tienda(idTienda)
    ON DELETE CASCADE,
)

go

create table TypeDocumentSale(
[idTypeDocumentSale] int primary key identity(1,1),
[description] varchar(50),
[isActive] bit,
web bit null,
tipoFactura int not null,
comision decimal(10,2) not null,
[registrationDate] datetime default getdate()
)

go

create table Sale(
[idSale] int primary key identity(1,1),
[saleNumber] varchar(6),
[idTypeDocumentSale] int references TypeDocumentSale(idTypeDocumentSale) null,
[customerDocument] varchar(10),
[clientName] varchar(20),
[total] decimal(10,2),
idTurno int references Turno(idTurno) not null,
IdTienda int not null,
idClienteMovimiento int null,
descuentoRecargo decimal(10,2) null,
IdFacturaEmitida int null,
isWeb bit null,
isDelete bit not null,
Observaciones varchar(max),
ResultadoFacturacion bit null,
[registrationDate] datetime default getdate(),
[registrationUser] varchar(50) not null
)

go

create table VentaWeb(
idVentaWeb int primary key identity(1,1),
[saleNumber] varchar(6),
Nombre varchar(100) null,
Telefono varchar(50) null,
Direccion varchar(100) null,
Comentario varchar(200) null,
[idFormaDePago] int references TypeDocumentSale(idTypeDocumentSale),
[Total] decimal(10,2) not null,
IdTienda int null,
estado int not null,
isEdit bit null,
editText varchar(max) null,
descuentoRetiroLocal decimal(10,2) null,
cruceCallesDireccion varchar(max) null,
CostoEnvio decimal(10,2) null,
ObservacionesUsuario varchar(max) null,
idSale int references Sale(idSale) null,
[registrationDate] datetime not null,
[modificationDate] datetime null,
[modificationUser] varchar(50) null,
)

go

create table DetailSale(
[idDetailSale] int primary key identity(1,1),
[idSale] int references Sale(idSale),
[idProduct] int,
[brandProduct] varchar(100),
[descriptionProduct] varchar(100),
[categoryProducty] varchar(100),
[quantity] decimal(10,2),
promocion varchar(300) null,
[price] decimal(10,2),
[total] decimal(10,2),
tipoVenta int null,
iva decimal(10,2) null,
[idVentaWeb] int null references VentaWeb(idVentaWeb),
Recogido bit null
)

go


create table Cliente(
	[idCliente] int primary key identity(1,1),
	[nombre] varchar(150) not null,
	[cuil] varchar(50) null,
	[telefono] varchar(50) null,
	[direccion] varchar(200) null,
	IdTienda int references Tienda(idTienda) not null,
	[isActive] bit,
	CondicionIva int null,
	Comentario varchar(200) null ,
	IdFacturaEmitida int null,
	[registrationDate] datetime not null,
	[modificationDate] datetime null,
	[modificationUser] varchar(50) null
)

go

create table ClienteMovimiento(
	[idClienteMovimiento] int primary key identity(1,1),
	[idCliente] int references Cliente(idCliente) not null,
	[idSale] int references Sale(idSale) null,
	[total] decimal(10,2) not null,
	IdTienda int references Tienda(idTienda) not null,
	TipoMovimiento int not null,
	[registrationDate] datetime not null,
	[registrationUser] varchar(50) not null
)



go

create table Promocion(
	[idPromocion] int primary key identity(1,1),
	[nombre] varchar(100) null,
	[idProducto] varchar(100) null,
	[operador] int null,
	[cantidadProducto] int null,
	[idCategory] varchar(100) null,
	[dias] varchar(100) null,
	[precio] decimal(10,2) null,
	[porcentaje] decimal(10,2) null,
	IdTienda int references Tienda(idTienda) not null,
	[isActive] bit,
	[registrationDate] datetime not null,
	[modificationDate] datetime null,
	[modificationUser] varchar(50) null,
)

go

create table ProveedorMovimiento(
	[idProveedorMovimiento] int primary key identity(1,1),
	[idProveedor] int references Proveedor(idProveedor) not null,
	[importe] decimal(10,2) not null,
	[importeSinIva] decimal(10,2) null,
	[Iva] decimal(10,2) null,
	[Ivaimporte] decimal(10,2) null,
	[nroFactura] varchar(50) null,
	[tipoFactura] varchar(50) null,
	[comentario] varchar(300)  null,
	idTienda int not null,
	EstadoPago int not null,
	FacturaPendiente bit not null,
	[modificationDate] [datetime] null,
	[modificationUser] varchar(50) null,
	[registrationDate] datetime not null,
	[registrationUser] varchar(50) not null
)

go

create table TipoGastos(
	[idTipoGastos] int primary key identity(1,1),
	[gastoParticular] int not null,
	[descripcion] varchar(150) not null,
	iva decimal(10,2) null,
	tipoFactura int null
)

go

create table Gastos(
	[idGastos] int primary key identity(1,1),
	[idTipoGasto] int references TipoGastos(idTipoGastos) not null,
	[importe] decimal(10,2) not null,
	[importeSinIva] decimal(10,2) null,
	[Iva] decimal(10,2),
	[Ivaimporte] decimal(10,2) null,
	[nroFactura] varchar(50) null,
	[tipoFactura] varchar(50) null,
	[comentario] varchar(300) null,
	[idTienda] int not null,
	EstadoPago int not null,
	FacturaPendiente bit not null,
	GastoAsignado varchar(150) null,
	[registrationDate] datetime not null,
	[registrationUser] varchar(50) not null,
	[modificationDate] [datetime] null,
	[modificationUser] varchar(50) null
)

go

create table AuditoriaModificaciones(
idAuditoriaModificaciones int primary key identity(1,1),
entidad varchar(50) not null,
idEntidad int not null,
descripcion varchar(150) not null,
entidadAntes varchar(max) not null,
entidadDespues varchar(max) not null,
[modificationDate] [datetime] null,
[modificationUser] varchar(50) null,
)

go

create table Notifications(
idNotifications int primary key identity(1,1),
descripcion varchar(max) not null,
[isActive] bit not null,
accion varchar(100) null,
[rols] varchar(20) not null,
registrationDate [datetime] not null,
[modificationDate] [datetime] null,
[modificationUser] varchar(50) null
)

go

create table ListaPrecios(
[idListaPrecios] int primary key identity(1,1),
lista int not null,
[idProducto] int references Product(idProduct) not null,
[precio] decimal(10,2) not null,
porcentajeProfit int not null,
[registrationDate] datetime default getdate()
)

go

create table Vencimientos(
idVencimiento int primary key identity(1,1),
lote varchar(100) null,
fechaVencimiento datetime not null,
fechaElaboracion datetime null,
notificar bit not null,
idProducto int references Product(idProduct) not null,
idTienda int references Tienda(idTienda) not null,
registrationDate datetime default getdate(),
registrationUser varchar(50),
)

go

create table Pedidos(
[idPedido] int primary key identity(1,1),
[importeEstimado] decimal(10,2) null,
estado int not null,
[comentario] varchar(200) null,
[fechaRecibido] datetime null,
[idProveedorMovimiento] int references ProveedorMovimiento(idProveedorMovimiento) null,
[idProveedor] int references Proveedor(idProveedor),
[idTienda] int references Tienda(idTienda),
fechaCerrado datetime  null,
usuarioFechaCerrado varchar(100)  null,
importeFinal decimal(10,2)  null,
[registrationDate] datetime default getdate(),
[registrationUser] varchar(50) not null
)

go

create table PedidoProducto(
[IdPedidoProducto] int primary key identity(1,1),
cantidadProducto int not null,
lote varchar(100) null,
vencimiento datetime null,
cantidadProductoRecibida int  null,
idProducto int references Product(idProduct) not null,
idPedido int references Pedidos(idPedido) not null,
)

go

alter table [dbo].[ProveedorMovimiento] add IdPedido int references Pedidos(IdPedido) null

go

create table AjustesWeb(
idAjusteWeb int primary key identity(1,1),
MontoEnvioGratis decimal(10,2) null,
AumentoWeb decimal(10,2) null,
CostoEnvio decimal(10,2) null,
CompraMinima decimal(10,2) null,
TakeAwayDescuento decimal(10,2) null,
HabilitarTakeAway BIT NULL,
Whatsapp varchar(50) null,
Lunes varchar(100) null,
Martes varchar(100) null,
Miercoles varchar(100) null,
Jueves varchar(100) null,
Viernes varchar(100) null,
Sabado varchar(100) null,
Domingo varchar(100) null,
Feriado varchar(100) null,
Facebook varchar(50) null,
Instagram varchar(50) null,
Tiktok varchar(50) null,
Twitter varchar(50) null,
Youtube varchar(80) null,
direccion varchar(50) null,
telefono varchar(50) null,
nombre varchar(50) null,
habilitarWeb BIT NULL,
NombreComodin1 varchar(150) null,
HabilitarComodin1 BIT NULL,
NombreComodin2 varchar(150) null,
HabilitarComodin2 BIT NULL,
NombreComodin3 varchar(150) null,
HabilitarComodin3 BIT NULL,
[modificationDate] [datetime] null,
[modificationUser] varchar(50) null
)

go

CREATE TABLE Ajustes (
idAjuste INT PRIMARY KEY IDENTITY(1,1),
codigoSeguridad VARCHAR(20) NULL,
ImprimirDefault BIT NULL,
Encabezado1 VARCHAR(100) NULL,
Encabezado2 VARCHAR(100) NULL,
Encabezado3 VARCHAR(100) NULL,
Pie1 VARCHAR(100) NULL,
Pie2 VARCHAR(200) NULL,
Pie3 VARCHAR(200) NULL,
NombreImpresora VARCHAR(500) NULL,
MinimoIdentificarConsumidor BIGINT NULL,
ControlStock BIT NULL,
FacturaElectronica BIT NULL,
idTienda INT NOT NULL,
ControlEmpleado BIT NULL,
NotificarEmailCierreTurno BIT NULL,
EmailEmisorCierreTurno VARCHAR(50) NULL,
PasswordEmailEmisorCierreTurno VARCHAR(max) NULL,
EmailsReceptoresCierreTurno VARCHAR(50) NULL,
modificationDate DATETIME NULL,
modificationUser VARCHAR(50) NULL,
CONSTRAINT FK_Ajustes_Tienda FOREIGN KEY (idTienda)
    REFERENCES Tienda(idTienda)
    ON DELETE CASCADE
);
go

CREATE TABLE AjustesFacturacion (
idAjustesFacturacion INT PRIMARY KEY IDENTITY(1,1),
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
IsProdEnvironment BIT not null,
modificationDate DATETIME NULL,
modificationUser VARCHAR(50) NULL,
CONSTRAINT FK_AjustesFacturacion_Tienda FOREIGN KEY (idTienda)
    REFERENCES Tienda(idTienda)
    ON DELETE CASCADE
);

go

alter table Tienda add idAjustes int references Ajustes(idAjuste) null;
alter table Tienda add IdAjustesFacturacion int references AjustesFacturacion(IdAjustesFacturacion) null;
alter table Tienda add IdCorrelativeNumber int references CorrelativeNumber(IdCorrelativeNumber) null;

go

create table Stock(
idStock int primary key identity(1,1),
StockActual decimal(10,2) not null,
StockMinimo int not null,
[idProducto] int references Product(idProduct) not null,
[idTienda] int references Tienda(idTienda) not null,
)

go


CREATE TABLE FacturasEmitidas (
    IdFacturaEmitida INT IDENTITY(1,1) PRIMARY KEY,
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
    Observaciones VARCHAR(max), 
	ImporteTotal decimal(10,2) not null,
	ImporteNeto decimal(10,2) not null,
	ImporteIva decimal(10,2) not null,
	IdFacturaAnulada int null,
    FacturaAnulada varchar(50) null,
	idSale int references Sale(idSale) not null,
	idCliente int references Cliente(idCliente) null,
	IdTienda int references Tienda(IdTienda) not null,
    RegistrationUser VARCHAR(200),
    RegistrationDate DATETIME
);

go

CREATE TABLE CodigoBarras (
    IdCodigoBarras INT IDENTITY(1,1) PRIMARY KEY,
    Codigo VARCHAR(50) not null,
    Descripcion VARCHAR(50) null,
	[idProducto] int references Product(idProduct) not null
);

go

-- Crear la tabla RazonMovimientoCaja
CREATE TABLE RazonMovimientoCaja (
    IdRazonMovimientoCaja INT PRIMARY KEY IDENTITY(1,1),
    Descripcion NVARCHAR(255) NOT NULL,
    Tipo INT NOT NULL,
	estado BIT not null
);

go

-- Crear la tabla MovimientoCaja
CREATE TABLE MovimientoCaja (
    IdMovimientoCaja INT PRIMARY KEY IDENTITY(1,1),
    Comentario NVARCHAR(500) NULL,
    RegistrationDate DATETIME NOT NULL,
    RegistrationUser NVARCHAR(50) NOT NULL,
	importe decimal(10,2),
    IdRazonMovimientoCaja INT NOT NULL,
	idTienda int not null,
	idTurno int not null,
    CONSTRAINT FK_MovimientoCaja_RazonMovimientoCaja FOREIGN KEY (IdRazonMovimientoCaja) REFERENCES RazonMovimientoCaja(IdRazonMovimientoCaja)
);

go

CREATE TABLE Tags (
    IdTag INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(20) NOT NULL,
    Color NVARCHAR(7) NOT NULL
);

go

CREATE TABLE ProductTags (
    ProductId INT NOT NULL,
    TagId INT NOT NULL,
    PRIMARY KEY (ProductId, TagId),
    FOREIGN KEY (ProductId) REFERENCES Product(IdProduct) ON DELETE CASCADE,
    FOREIGN KEY (TagId) REFERENCES Tags(IdTag) ON DELETE CASCADE
);

CREATE TABLE Lov (
    Id INT PRIMARY KEY IDENTITY(1,1),
	descripcion varchar(50) NOT NULL,
	estado bit NOT NULL,
    LovType INT NOT NULL,
	[registrationDate] datetime null,
	RegistrationUser varchar(150),
	[modificationDate] [datetime] null,
	[modificationUser] varchar(50) null
);

go

CREATE TABLE Horario (
    Id INT PRIMARY KEY IDENTITY(1,1),  
    horaEntrada NVARCHAR(5) NOT NULL,         
    horaSalida NVARCHAR(5) NOT NULL,          
    DiaSemana INT NOT NULL,                   
    IdUsuario INT NOT NULL,                   
    RegistrationDate DATETIME, 
	RegistrationUser varchar(150),
    ModificationDate DATETIME,                
    ModificationUser varchar(150),           
	FOREIGN KEY (IdUsuario) REFERENCES Users([idUsers]) ON DELETE CASCADE
);

go 

CREATE TABLE Empresa (
    Id INT PRIMARY KEY IDENTITY(1,1),  
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

go 

CREATE TABLE PagoEmpresa (
    Id INT PRIMARY KEY IDENTITY(1,1),  
    FechaPago DATETIME NOT NULL, 
    Importe decimal(10,2) NOT NULL,
	Comentario varchar(300) NULL,
	EstadoPago int NULL,
	[importeSinIva] decimal(10,2) null,
	[Iva] decimal(10,2) null,
	[Ivaimporte] decimal(10,2) null,
	[nroFactura] varchar(50) null,
	[tipoFactura] int null,
	FacturaPendiente bit null,
	IdEmpresa INT NOT NULL,
    RegistrationDate DATETIME,     
	RegistrationUser varchar(150),
    ModificationDate DATETIME,                
    ModificationUser varchar(150),
	FOREIGN KEY (IdEmpresa) REFERENCES Empresa(Id) ON DELETE CASCADE
);

go 

CREATE TABLE ProductLov
(
    ProductId INT NOT NULL,
    LovId INT NOT NULL,
    LovType INT NOT NULL,

    CONSTRAINT PK_ProductLov PRIMARY KEY (ProductId, LovId, LovType),

    CONSTRAINT FK_ProductLov_Product FOREIGN KEY (ProductId) 
        REFERENCES Product(IdProduct) ON DELETE CASCADE,

    CONSTRAINT FK_ProductLov_Lov FOREIGN KEY (LovId) 
        REFERENCES Lov(Id) ON DELETE CASCADE
);

go 

create table BackupProducto(
Id int primary key identity(1,1), 
CorrelativeNumberMasivo VARCHAR(10) null,
RegistrationUser varchar(50) null,
RegistrationDate datetime null,
[idProduct] int,
[description] varchar(100),
[price] decimal(10,2),
[photo] varbinary(max),
[isActive] bit,
priceWeb decimal(10,2) null,
precioFormatoWeb decimal(10,2) null,
formatoWeb int null,
porcentajeProfit int null,
costPrice decimal(10,2) null,
tipoVenta int not null,
[comentario] varchar(300) null,
iva decimal(10,2) null,
destacado bit null,
productoWeb bit null,
modificarPrecio bit null,
PrecioAlMomento bit null,
ExcluirPromociones bit null,
[Category] varchar(100) null,
[Proveedor] varchar(100) null,
Precio1 decimal(10,2) null,
PorcentajeProfit1 int null,
Precio2 decimal(10,2) null,
PorcentajeProfit2 int null,
Precio3 decimal(10,2) null,
PorcentajeProfit3 int null,
sku varchar(50) null
)

go

CREATE TABLE VentasPorTipoDeVentaTurno (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Descripcion NVARCHAR(MAX) NULL,
    TotalSistema DECIMAL(18, 2) NULL,
    TotalUsuario DECIMAL(18, 2) NULL,
    Error NVARCHAR(MAX) NULL,
    IdTurno INT NOT NULL,

    CONSTRAINT FK_VentasPorTipoDeVentaTurno_Turno FOREIGN KEY (IdTurno) REFERENCES Turno(IdTurno)
);

go

CREATE TABLE HistorialLogin (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Informacion NVARCHAR(MAX) NOT NULL,
    Fecha DATETIME NOT NULL,
    IdUser INT NOT NULL,
    UserName NVARCHAR(255) NOT NULL,
    CONSTRAINT FK_HistorialLogin_User FOREIGN KEY (IdUser)
        REFERENCES Users(idUsers)
        ON DELETE CASCADE
);

