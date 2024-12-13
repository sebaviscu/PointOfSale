
TRUNCATE TABLE DetailSale
TRUNCATE TABLE FacturasEmitidas
TRUNCATE TABLE VentaWeb

-- Deshabilitar temporalmente las restricciones de clave foránea 
ALTER TABLE VentaWeb NOCHECK CONSTRAINT ALL 
-- Eliminar todas las filas de la tabla 
DELETE FROM VentaWeb 
-- Reiniciar el valor autoincremental 
DBCC CHECKIDENT (VentaWeb, RESEED, 0) 
-- Habilitar de nuevo las restricciones de clave foránea 
ALTER TABLE VentaWeb WITH CHECK CHECK CONSTRAINT ALL

-- Deshabilitar temporalmente las restricciones de clave foránea 
ALTER TABLE Sale NOCHECK CONSTRAINT ALL 
-- Eliminar todas las filas de la tabla 
DELETE FROM Sale 
-- Reiniciar el valor autoincremental 
DBCC CHECKIDENT (Sale, RESEED, 0) 
-- Habilitar de nuevo las restricciones de clave foránea 
ALTER TABLE Sale WITH CHECK CHECK CONSTRAINT ALL

TRUNCATE TABLE ClienteMovimiento

-- Deshabilitar temporalmente las restricciones de clave foránea 
ALTER TABLE ProveedorMovimiento NOCHECK CONSTRAINT ALL 
-- Eliminar todas las filas de la tabla 
DELETE FROM ProveedorMovimiento 
-- Reiniciar el valor autoincremental 
DBCC CHECKIDENT (ProveedorMovimiento, RESEED, 0) 
-- Habilitar de nuevo las restricciones de clave foránea 
ALTER TABLE ProveedorMovimiento WITH CHECK CHECK CONSTRAINT ALL

TRUNCATE TABLE Gastos
TRUNCATE TABLE Notifications
TRUNCATE TABLE ListaPrecios
TRUNCATE TABLE Vencimientos
TRUNCATE TABLE PedidoProducto

-- Deshabilitar temporalmente las restricciones de clave foránea 
ALTER TABLE Pedidos NOCHECK CONSTRAINT ALL 
-- Eliminar todas las filas de la tabla 
DELETE FROM Pedidos 
-- Reiniciar el valor autoincremental 
DBCC CHECKIDENT (Pedidos, RESEED, 0) 
-- Habilitar de nuevo las restricciones de clave foránea 
ALTER TABLE Pedidos WITH CHECK CHECK CONSTRAINT ALL

TRUNCATE TABLE Stock
TRUNCATE TABLE CodigoBarras
TRUNCATE TABLE MovimientoCaja
TRUNCATE TABLE ProductTags
TRUNCATE TABLE ProductLov
TRUNCATE TABLE BackupProducto
TRUNCATE TABLE VentasPorTipoDeVentaTurno

-- Deshabilitar temporalmente las restricciones de clave foránea 
ALTER TABLE Turno NOCHECK CONSTRAINT ALL 
-- Eliminar todas las filas de la tabla 
DELETE FROM Turno 
-- Reiniciar el valor autoincremental 
DBCC CHECKIDENT (Turno, RESEED, 0) 
-- Habilitar de nuevo las restricciones de clave foránea 
ALTER TABLE Turno WITH CHECK CHECK CONSTRAINT ALL


-- Deshabilitar temporalmente las restricciones de clave foránea 
ALTER TABLE Product NOCHECK CONSTRAINT ALL 
-- Eliminar todas las filas de la tabla 
DELETE FROM Product 
-- Reiniciar el valor autoincremental 
DBCC CHECKIDENT (Product, RESEED, 0) 
-- Habilitar de nuevo las restricciones de clave foránea 
ALTER TABLE Product WITH CHECK CHECK CONSTRAINT ALL

update CorrelativeNumber set [lastNumber] = 0

