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
  Telefono VARCHAR(20) NOT NULL,
  Correo VARCHAR(255) NOT NULL,
  Dui VARCHAR(50) NOT NULL,
  PRIMARY KEY (id)
);

INSERT INTO usuarios (nombre, usuario, rol, clave, Telefono, Correo, Dui)
VALUES ('Administrador', 'admin', 1, '123', '2356-8900', 'admin@example.com', '12345678-9');

INSERT INTO usuarios (nombre, usuario, rol, clave, Telefono, Correo, Dui)
VALUES ('Fatima Alexandra', 'fatima', 2, '123', '6126-2117', 'fatt.alexandra@gmail.com', '00598478-9');

INSERT INTO usuarios (nombre, usuario, rol, clave, Telefono, Correo, Dui)
VALUES ('Fernando Salinas', 'fernando', 2, '123', '6560-2517', 'cm17102@ues.edu.sv', '00002458-9');

INSERT INTO usuarios (nombre, usuario, rol, clave, Telefono, Correo, Dui)
VALUES ('Maria Cortez', 'maria', 3, '123', '7894-8974', 'fatt.alexandra@gmail.com', '00598785-9');
INSERT INTO usuarios (nombre, usuario, rol, clave, Telefono, Correo, Dui)
VALUES ('Luis Martinez', 'luis', 3, '123', '7878-0074', 'fatt.alexandra@gmail.com', '02528585-9');
INSERT INTO usuarios (nombre, usuario, rol, clave, Telefono, Correo, Dui)
VALUES ('Ronald Hernandez', 'ronald', 3, '123', '604-1973', 'fatt.alexandra@gmail.com', '1236705-9');

CREATE TABLE Servicios
(
  id INT NOT NULL IDENTITY(1,1),
  nombre VARCHAR(255) NOT NULL,
  descripcion VARCHAR(255),
  categoria VARCHAR(50),
  PRIMARY KEY (id)
);
CREATE TABLE Vehiculos
(
  id INT NOT NULL IDENTITY(1,1),
  UsuarioId INT NOT NULL,
  Marca VARCHAR(50) NOT NULL,
  Modelo VARCHAR(50) NOT NULL,
  Año INT NOT NULL,
  Placa VARCHAR(20) NOT NULL,
  Descripcion VARCHAR(255), 
  PRIMARY KEY (id),
  FOREIGN KEY (UsuarioId) REFERENCES usuarios(id),
);

Select * from Vehiculos;

CREATE TABLE ServicioRealizado
(
  id INT NOT NULL IDENTITY(1,1),
  ServicioId INT NOT NULL,
  UsuarioId INT NOT NULL,
  Precio DECIMAL(18, 2) NOT NULL,
  Fecha VARCHAR(50) NOT NULL,
  Estado INT NOT NULL DEFAULT 1,
  VehiculoId INT NULL,
  PRIMARY KEY (id),
  FOREIGN KEY (ServicioId) REFERENCES Servicios(id),
  FOREIGN KEY (UsuarioId) REFERENCES usuarios(id),
  FOREIGN KEY (VehiculoId) REFERENCES Vehiculos(id)
);
create table productos
(
	id INT NOT NULL IDENTITY(1,1),
	nombre VARCHAR(255) NOT NULL,
	descripcion VARCHAR(255) NOT NULL,
	tipo VARCHAR(255) NOT NULL,
	precio DECIMAL (18,2) NOT NULL
);

select * from Servicios
select * from ServicioRealizado
select * from usuarios
select * from productos

-- Insertar servicios
INSERT INTO Servicios (nombre, descripcion, categoria) VALUES
('Limpieza de frenos', 'Limpieza completa de frenos de automóvil', 'Mecánica');

INSERT INTO Servicios (nombre, descripcion, categoria) VALUES
('Cambio de aceite', 'Cambio de aceite y filtro para automóviles', 'Mecánica');

INSERT INTO Servicios (nombre, descripcion, categoria) 
VALUES ('Alineación de ruedas', 'Ajuste de la alineación de las ruedas de automóviles', 'Mecánica');

INSERT INTO Servicios (nombre, descripcion, categoria) 
VALUES ('Revisión de frenos', 'Inspección y ajuste de frenos para automóviles', 'Mecánica');

INSERT INTO Servicios (nombre, descripcion, categoria) 
VALUES ('Lavado interior', 'Limpieza profunda del interior de vehículos', 'Lavado de autos');

INSERT INTO Servicios (nombre, descripcion, categoria) 
VALUES ('Pintura de carrocería', 'Reparación y repintado de la carrocería de automóviles', 'Carrocería');

INSERT INTO Servicios (nombre, descripcion, categoria) 
VALUES ('Instalación de audio', 'Instalación de sistemas de audio personalizados para vehículos', 'Electrónica');

-------------------------------------------------------------------------------------------------------------
-- Insertar datos de ejemplo en la tabla Vehiculos 
INSERT INTO Vehiculos (UsuarioId, Marca, Modelo, Año, Placa, Descripcion)
VALUES (2, 'Toyota', 'Corolla', 2006, 'P123-456', 'Sedán blanco, bien mantenido');

INSERT INTO Vehiculos (UsuarioId, Marca, Modelo, Año, Placa, Descripcion)
VALUES (3, 'Honda', 'Civic', 2018, 'P987-654', 'Auto compacto, color negro');

INSERT INTO Vehiculos (UsuarioId, Marca, Modelo, Año, Placa, Descripcion)
VALUES (4, 'Ford', 'Focus', 2015, 'P321-789', 'Hatchback gris, con rines personalizados');

INSERT INTO Vehiculos (UsuarioId, Marca, Modelo, Año, Placa, Descripcion)
VALUES (5, 'Chevrolet', 'Spark', 2017, 'P555-111', 'Auto pequeño, color rojo, económico');

-- Insertar servicios realizados
INSERT INTO ServicioRealizado (ServicioId, UsuarioId, Precio, Fecha, Estado, VehiculoId)
VALUES (3, 3, 50.00, '2024-06-16', 1, 3);

-- Insertar 
INSERT INTO ServicioRealizado (ServicioId, UsuarioId, Precio, Fecha, Estado, VehiculoId)
VALUES (1, 2, 30.00, '2024-06-15', 1, 2);

-- Insertar 
INSERT INTO ServicioRealizado (ServicioId, UsuarioId, Precio, Fecha, Estado, VehiculoId)
VALUES (2, 2, 50.00, '2024-06-16', 1, 2);

-- Insertar 
INSERT INTO ServicioRealizado (ServicioId, UsuarioId, Precio, Fecha, Estado, VehiculoId)
VALUES (4, 4, 30.00, '2024-06-15', 1, 4);

-- Insertar 
INSERT INTO ServicioRealizado (ServicioId, UsuarioId, Precio, Fecha, Estado, VehiculoId)
VALUES (1, 5, 50.00, '2024-06-14', 1, 4);

select * from Vehiculos
select * from ServicioRealizado


------------------------------------------------------------------------------------------






