

------------------CREACIÓN DE BADE DB Y TABLAS-----------------------------------
if db_id('dbtaller') is not null
	begin
		use master
		drop database dbtaller
	end

CREATE DATABASE dbtaller
go
USE dbtaller
go

CREATE TABLE usuarios
(
  id INT NOT NULL IDENTITY(1,1),
  nombre VARCHAR(255) NOT NULL,
  usuario VARCHAR(50) NOT NULL,
  rol INT NOT NULL,
  clave VARCHAR(255) NOT NULL,
  PRIMARY KEY (id)
);


---------------------------INSERSIÓN DE DATOS-----------------------------------------
go

-- Insertar datos en la tabla usuarios
INSERT INTO usuarios (nombre, usuario, rol, clave) VALUES('Administrador','admin',1,'admin')
INSERT INTO usuarios (nombre, usuario, rol, clave) VALUES('Fatima','fatima',1,'123')
INSERT INTO usuarios (nombre, usuario, rol, clave) VALUES('Ronald','ronald',2,'123')
go
select * from usuarios