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
CREATE TABLE ServicioRealizado
(
  id INT NOT NULL IDENTITY(1,1),
  ServicioId INT NOT NULL,
  UsuarioId INT NOT NULL,
  Precio DECIMAL(18, 2) NOT NULL,
  Fecha VARCHAR(50) NOT NULL,
  Estado INT NOT NULL DEFAULT 1,
  PRIMARY KEY (id),
  FOREIGN KEY (ServicioId) REFERENCES Servicios(id),
  FOREIGN KEY (UsuarioId) REFERENCES usuarios(id)
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

-- Insertar ejemplo 1
INSERT INTO ServicioRealizado (ServicioId, UsuarioId, Precio, Fecha, Estado)
VALUES (1, 4, 50.00, '2024-06-16', 1);

-- Insertar ejemplo 2
INSERT INTO ServicioRealizado (ServicioId, UsuarioId, Precio, Fecha, Estado)
VALUES (2, 4, 30.00, '2024-06-15', 1);

-- Insertar ejemplo 1
INSERT INTO ServicioRealizado (ServicioId, UsuarioId, Precio, Fecha, Estado)
VALUES (1, 5, 50.00, '2024-06-16', 1);

-- Insertar ejemplo 2
INSERT INTO ServicioRealizado (ServicioId, UsuarioId, Precio, Fecha, Estado)
VALUES (4, 5, 30.00, '2024-06-15', 1);

-- Insertar ejemplo 3
INSERT INTO ServicioRealizado (ServicioId, UsuarioId, Precio, Fecha, Estado)
VALUES (6, 3, 50.00, '2024-06-14', 1);
