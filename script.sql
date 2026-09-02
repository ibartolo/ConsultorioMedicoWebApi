

/****** Object:  Database [DB_9C7990_Consultorio]    Script Date: 24/08/2026 08:51:55 a. m. ******/
CREATE DATABASE [DB_9C7990_Consultorio] ON  PRIMARY 
( NAME = N'ConsultorioMedico', FILENAME = N'H:\Program Files\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER\MSSQL\DATA\DB_9C7990_Consultorio_DATA.mdf' , SIZE = 61504KB , MAXSIZE = 102400KB , FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'ConsultorioMedico_log', FILENAME = N'H:\Program Files\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER\MSSQL\DATA\\DB_9C7990_Consultorio_LOG.ldf' , SIZE = 576KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET COMPATIBILITY_LEVEL = 100
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [DB_9C7990_Consultorio].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET ARITHABORT OFF 
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET  DISABLE_BROKER 
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET  MULTI_USER 
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET DB_CHAINING OFF 
GO
EXEC sys.sp_db_vardecimal_storage_format N'DB_9C7990_Consultorio', N'ON'
GO
USE [DB_9C7990_Consultorio]
GO
/****** Object:  Table [dbo].[Anexo]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Anexo](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Estatus] [bit] NOT NULL,
	[Fecha] [datetime] NULL,
	[Comentarios] [nvarchar](1000) NULL,
	[PacienteID] [int] NOT NULL,
	[Nombre] [nvarchar](50) NULL,
	[Ruta] [nvarchar](200) NULL,
	[ConsultaID] [int] NULL,
 CONSTRAINT [PK__Anexo__3214EC071367E606] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Cita]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Cita](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Estatus] [bit] NOT NULL,
	[FechaCita] [datetime] NULL,
	[PacienteID] [int] NOT NULL,
	[Comentarios] [nvarchar](500) NULL,
	[TratamientoID] [int] NULL,
	[Estado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CitaPaquete]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CitaPaquete](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CitaID] [int] NULL,
	[PacientePaqueteID] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Consulta]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Consulta](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Estatus] [bit] NOT NULL,
	[FechaConsulta] [datetime] NULL,
	[PacienteID] [int] NOT NULL,
	[TratamientoID] [int] NULL,
	[CitaID] [int] NULL,
	[Descripcion] [nvarchar](500) NULL,
	[TipoConsultaID] [int] NULL,
	[DescripcionReceta] [nvarchar](1000) NULL,
	[Comentarios] [nvarchar](500) NULL,
	[FechaProximaConsulta] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DatosFiscales]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DatosFiscales](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RazonSocial] [nvarchar](100) NULL,
	[RFC] [nvarchar](100) NULL,
	[Telefono1] [nvarchar](50) NULL,
	[Telefono2] [nvarchar](50) NULL,
	[Email] [nvarchar](100) NULL,
	[Calle] [nvarchar](50) NULL,
	[NoInterior] [nvarchar](10) NULL,
	[NoExterior] [nvarchar](10) NULL,
	[Colonia] [nvarchar](50) NULL,
	[Deleacion] [nvarchar](50) NULL,
	[Municipio] [nvarchar](50) NULL,
	[Estado] [nvarchar](50) NULL,
	[CP] [int] NULL,
	[Pais] [nvarchar](50) NULL,
	[DatosInstitucionID] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DatosInstitucion]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DatosInstitucion](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](100) NULL,
	[NoTrabajadores] [int] NULL,
	[Telefono1] [nvarchar](50) NULL,
	[Telefono2] [nvarchar](50) NULL,
	[Email] [nvarchar](100) NULL,
	[Calle] [nvarchar](50) NULL,
	[NoInterior] [nvarchar](10) NULL,
	[NoExterior] [nvarchar](10) NULL,
	[Colonia] [nvarchar](50) NULL,
	[Deleacion] [nvarchar](50) NULL,
	[Municipio] [nvarchar](50) NULL,
	[Estado] [nvarchar](50) NULL,
	[CP] [int] NULL,
	[Pais] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Paciente]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Paciente](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](50) NOT NULL,
	[ApellidoPaterno] [nvarchar](50) NULL,
	[ApellidoMaterno] [nvarchar](50) NULL,
	[Email] [nvarchar](50) NULL,
	[FechaAlta] [datetime] NULL,
	[TelefonoCel] [nvarchar](100) NULL,
	[Comentarios] [nvarchar](1000) NULL,
	[Estatus] [bit] NULL,
	[Edad] [int] NULL,
	[Sexo] [nvarchar](15) NULL,
	[FechaNacimiento] [datetime] NULL,
	[CreatedBY] [nvarchar](25) NULL,
	[CreatedDT] [nvarchar](25) NULL,
	[UpdatedBY] [nvarchar](25) NULL,
	[UpdatedDT] [nvarchar](25) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PacientePaquete]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PacientePaquete](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PacienteID] [int] NULL,
	[PaqueteID] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PagoPaquete]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PagoPaquete](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Pago] [int] NULL,
	[FechaPago] [datetime] NULL,
	[PacientePaqueteID] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Paquete]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Paquete](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](100) NULL,
	[Descripcion] [nvarchar](500) NULL,
	[FechaCreacion] [datetime] NULL,
	[Estatus] [bit] NULL,
	[Costo] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TipoConsulta]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TipoConsulta](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](50) NOT NULL,
	[Duracion] [int] NULL,
	[Descripcion] [nvarchar](500) NULL,
	[Estatus] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tratamiento]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tratamiento](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](100) NULL,
	[Duracion] [int] NULL,
	[Descripcion] [varchar](500) NULL,
	[Estatus] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TratamientoPaquete]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TratamientoPaquete](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PaqueteID] [int] NULL,
	[TratamientoID] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Usuario]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Usuario](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](50) NULL,
	[Password] [nvarchar](50) NULL,
	[Estatus] [bit] NOT NULL,
	[CompaniaID] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  UserDefinedFunction [dbo].[PermisosPorCompania]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[PermisosPorCompania] (
    @CompaniaID		INT
)
RETURNS TABLE
AS
RETURN
	SELECT U.UserName
	FROM Usuario AS U
		INNER JOIN DatosInstitucion AS C ON U.CompaniaID = C.Id
	WHERE C.Id = @CompaniaID
GO
ALTER TABLE [dbo].[Anexo]  WITH CHECK ADD  CONSTRAINT [FK_Anexo_Consulta] FOREIGN KEY([ConsultaID])
REFERENCES [dbo].[Consulta] ([Id])
GO
ALTER TABLE [dbo].[Anexo] CHECK CONSTRAINT [FK_Anexo_Consulta]
GO
ALTER TABLE [dbo].[Anexo]  WITH CHECK ADD  CONSTRAINT [FK_Anexo_Paciente] FOREIGN KEY([PacienteID])
REFERENCES [dbo].[Paciente] ([Id])
GO
ALTER TABLE [dbo].[Anexo] CHECK CONSTRAINT [FK_Anexo_Paciente]
GO
ALTER TABLE [dbo].[Cita]  WITH CHECK ADD  CONSTRAINT [FK_Cita_Paciente] FOREIGN KEY([PacienteID])
REFERENCES [dbo].[Paciente] ([Id])
GO
ALTER TABLE [dbo].[Cita] CHECK CONSTRAINT [FK_Cita_Paciente]
GO
ALTER TABLE [dbo].[Cita]  WITH CHECK ADD  CONSTRAINT [FK_Cita_Tratamiento] FOREIGN KEY([TratamientoID])
REFERENCES [dbo].[Tratamiento] ([Id])
GO
ALTER TABLE [dbo].[Cita] CHECK CONSTRAINT [FK_Cita_Tratamiento]
GO
ALTER TABLE [dbo].[CitaPaquete]  WITH CHECK ADD  CONSTRAINT [FK_CitaPaquete_Cita] FOREIGN KEY([CitaID])
REFERENCES [dbo].[Paciente] ([Id])
GO
ALTER TABLE [dbo].[CitaPaquete] CHECK CONSTRAINT [FK_CitaPaquete_Cita]
GO
ALTER TABLE [dbo].[CitaPaquete]  WITH CHECK ADD  CONSTRAINT [FK_CitaPaquete_PacientePaquete] FOREIGN KEY([PacientePaqueteID])
REFERENCES [dbo].[Paquete] ([Id])
GO
ALTER TABLE [dbo].[CitaPaquete] CHECK CONSTRAINT [FK_CitaPaquete_PacientePaquete]
GO
ALTER TABLE [dbo].[Consulta]  WITH CHECK ADD  CONSTRAINT [FK_Consulta_Cita] FOREIGN KEY([CitaID])
REFERENCES [dbo].[Cita] ([Id])
GO
ALTER TABLE [dbo].[Consulta] CHECK CONSTRAINT [FK_Consulta_Cita]
GO
ALTER TABLE [dbo].[Consulta]  WITH CHECK ADD  CONSTRAINT [FK_Consulta_Paciente] FOREIGN KEY([PacienteID])
REFERENCES [dbo].[Paciente] ([Id])
GO
ALTER TABLE [dbo].[Consulta] CHECK CONSTRAINT [FK_Consulta_Paciente]
GO
ALTER TABLE [dbo].[Consulta]  WITH CHECK ADD  CONSTRAINT [FK_Consulta_TipoConsulta] FOREIGN KEY([TipoConsultaID])
REFERENCES [dbo].[TipoConsulta] ([Id])
GO
ALTER TABLE [dbo].[Consulta] CHECK CONSTRAINT [FK_Consulta_TipoConsulta]
GO
ALTER TABLE [dbo].[Consulta]  WITH CHECK ADD  CONSTRAINT [FK_Consulta_Tratamiento] FOREIGN KEY([TratamientoID])
REFERENCES [dbo].[Tratamiento] ([Id])
GO
ALTER TABLE [dbo].[Consulta] CHECK CONSTRAINT [FK_Consulta_Tratamiento]
GO
ALTER TABLE [dbo].[DatosFiscales]  WITH CHECK ADD  CONSTRAINT [FK_DatosFiscales_DatosInstitucion] FOREIGN KEY([DatosInstitucionID])
REFERENCES [dbo].[DatosInstitucion] ([Id])
GO
ALTER TABLE [dbo].[DatosFiscales] CHECK CONSTRAINT [FK_DatosFiscales_DatosInstitucion]
GO
ALTER TABLE [dbo].[PacientePaquete]  WITH CHECK ADD  CONSTRAINT [FK_PacientePaquete_Paciente] FOREIGN KEY([PacienteID])
REFERENCES [dbo].[Paciente] ([Id])
GO
ALTER TABLE [dbo].[PacientePaquete] CHECK CONSTRAINT [FK_PacientePaquete_Paciente]
GO
ALTER TABLE [dbo].[PacientePaquete]  WITH CHECK ADD  CONSTRAINT [FK_PacientePaquete_Paquete] FOREIGN KEY([PaqueteID])
REFERENCES [dbo].[Paquete] ([Id])
GO
ALTER TABLE [dbo].[PacientePaquete] CHECK CONSTRAINT [FK_PacientePaquete_Paquete]
GO
ALTER TABLE [dbo].[PagoPaquete]  WITH CHECK ADD  CONSTRAINT [FK_PagoPaquete_PacientePaquete] FOREIGN KEY([PacientePaqueteID])
REFERENCES [dbo].[PacientePaquete] ([Id])
GO
ALTER TABLE [dbo].[PagoPaquete] CHECK CONSTRAINT [FK_PagoPaquete_PacientePaquete]
GO
ALTER TABLE [dbo].[TratamientoPaquete]  WITH CHECK ADD  CONSTRAINT [FK_TratamientoPaquete_Paquete] FOREIGN KEY([PaqueteID])
REFERENCES [dbo].[Paquete] ([Id])
GO
ALTER TABLE [dbo].[TratamientoPaquete] CHECK CONSTRAINT [FK_TratamientoPaquete_Paquete]
GO
ALTER TABLE [dbo].[TratamientoPaquete]  WITH CHECK ADD  CONSTRAINT [FK_TratamientoPaquete_Tratamiento] FOREIGN KEY([TratamientoID])
REFERENCES [dbo].[Tratamiento] ([Id])
GO
ALTER TABLE [dbo].[TratamientoPaquete] CHECK CONSTRAINT [FK_TratamientoPaquete_Tratamiento]
GO
ALTER TABLE [dbo].[Usuario]  WITH CHECK ADD  CONSTRAINT [FK_Usuario_DatosInstitucion] FOREIGN KEY([CompaniaID])
REFERENCES [dbo].[DatosInstitucion] ([Id])
GO
ALTER TABLE [dbo].[Usuario] CHECK CONSTRAINT [FK_Usuario_DatosInstitucion]
GO
/****** Object:  StoredProcedure [dbo].[Autentication]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Autentication]
(
	@UserName	NVARCHAR(50),
	@Password	NVARCHAR(50)
)
AS
BEGIN

	SELECT *
	FROM Usuario
	WHERE UserName = @UserName AND Password = @Password AND Estatus = 1

END
GO
/****** Object:  StoredProcedure [dbo].[GetAllAnexoByPacienteID]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[GetAllAnexoByPacienteID]
(
@IdPaciente int
)
AS
BEGIN
	SELECT  A.Id,
			A.Estatus,
			A.Fecha,
			A.Comentarios,
			A.Nombre,
			A.Ruta		
	FROM Anexo A
		INNER JOIN Paciente P ON P.Id = A.PacienteID
		LEFT JOIN Consulta CO ON A.ConsultaId = CO.Id
	WHERE a.pacienteID = @IdPaciente or CO.Estatus = 1 and A.ConsultaId is null
	ORDER BY A.Fecha DESC
END
GO
/****** Object:  StoredProcedure [dbo].[GetAllCitaByPacienteID]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[GetAllCitaByPacienteID]
(
@IdPaciente int
)
AS
BEGIN
	Select  C.[Id],C.[FechaCita],C.[Comentarios],T.[Nombre],
			C.[Estatus],C.[PacienteID],
			C.[TratamientoID],
			C.[Estado]
	FROM Cita C
		INNER JOIN Paciente P ON P.Id = C.PacienteID
		LEFT JOIN Tratamiento T ON T.Id = C.TratamientoID
	WHERE C.PacienteID = @IdPaciente AND C.Estatus = 1 and P.Estatus = 1
	ORDER BY C.[FechaCita] DESC
END

GO
/****** Object:  StoredProcedure [dbo].[GetAllCitaByTratamientoId]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =================================================================================================================      
-- Author:		Byron David Rodriguez Herrera
-- Create date: 07/05/2015
-- Description: Regresa un listado de Citas en base a un Tratamiento seleccionado     
-- =================================================================================================================      
CREATE PROC [dbo].[GetAllCitaByTratamientoId]
(
	@TratamiendoId INT	
)
AS
BEGIN
SELECT  P.Id, P.Nombre + ' ' + P.ApellidoPaterno + ' ' + P.ApellidoMaterno AS NombreCompleto,
		P.TelefonoCel, C.Id AS CitaId, C.FechaCita AS UltimaCita, C.Comentarios AS ComentariosCita,
		T.Nombre AS Tratamiento, C.Estado, 
		'' as EstadoCita
FROM Paciente P
	INNER JOIN Cita C ON P.Id = C.PacienteId
	INNER JOIN Tratamiento T ON  T.Id = C.TratamientoId
WHERE P.Estatus = 1 AND C.Estatus = 1 AND T.Id = @TratamiendoId					 --Obtiene la fecha Actual
ORDER BY C.Estado,C.FechaCita DESC
END
GO
/****** Object:  StoredProcedure [dbo].[GetAllCitaByTratamientoIdForPublicidad]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[GetAllCitaByTratamientoIdForPublicidad]
(
	@TratamiendoId INT	
)
AS
BEGIN
	SELECT  P.Id, P.Nombre + ' ' + P.ApellidoPaterno + ' ' + P.ApellidoMaterno AS NombreCompleto,
			P.TelefonoCel, C.Id AS CitaId, C.FechaCita AS UltimaCita, C.Comentarios AS ComentariosCita,
			T.Nombre AS Tratamiento, C.Estado, isnull(P.Email,'') AS Email
	FROM Paciente P
		INNER JOIN Cita C ON P.Id = C.PacienteId
		INNER JOIN Tratamiento T ON  T.Id = C.TratamientoId
	WHERE P.Estatus = 1 AND C.Estatus = 1 AND T.Id = @TratamiendoId					 --Obtiene la fecha Actual
	ORDER BY C.Estado,C.FechaCita DESC
END
GO
/****** Object:  StoredProcedure [dbo].[GetAllCitaTratamiento]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetAllCitaTratamiento]
AS  
BEGIN  
  
	SELECT C.*, P.Nombre, P.ApellidoPaterno, P.ApellidoMaterno, T.Nombre AS TratamientoNombre, t.Duracion  
	FROM Cita AS C  
		INNER JOIN Paciente AS P ON C.PacienteID = P.Id  
		INNER JOIN Tratamiento AS T ON C.TratamientoID = T.Id  
	WHERE C.Estatus = 1 and P.Estatus = 1  
  
END
GO
/****** Object:  StoredProcedure [dbo].[GetAllConsultaByPacienteID]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[GetAllConsultaByPacienteID]
(
@IdPaciente int
)
AS
BEGIN
Select  CO.Id,
		CO.Estatus,
		CO.FechaConsulta,
		CO.Descripcion,
		CO.PacienteID,
		CO.TratamientoID,
		CO.CitaID,
		CO.TipoConsultaID,
		TC.Nombre,
		CO.DescripcionReceta
	FROM Consulta CO
		INNER JOIN Paciente P ON P.Id = CO.PacienteID
		LEFT JOIN TipoConsulta TC ON TC.Id = CO.TipoCOnsultaId
		--INNER JOIN Anexo A ON A.ConsultaID = CO.Id
	WHERE CO.PacienteID = @IdPaciente AND CO.Estatus = 1
	ORDER BY CO.FechaConsulta DESC
END
GO
/****** Object:  StoredProcedure [dbo].[GetAllConsultaByTipoConsultaId]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =================================================================================================================      
-- Author:		Byron David Rodriguez Herrera
-- Create date: 07/05/2015
-- Description: Regresa un listado de Consultas en base a un TipoConsulta seleccionado     
-- =================================================================================================================      
CREATE PROC [dbo].[GetAllConsultaByTipoConsultaId]
(
	@TipoConsultaId INT	
)
AS
BEGIN
	SELECT  P.Id, P.Nombre + ' ' + P.ApellidoPaterno + ' ' + P.ApellidoMaterno AS NombreCompleto,
			P.TelefonoCel, C.Id AS ConsultaId, C.FechaConsulta AS UltimaConsulta, C.Descripcion AS DescripcionConsulta,
			T.Nombre AS Consulta
	FROM Paciente P
	INNER JOIN Consulta C ON P.Id = C.PacienteId
	INNER JOIN TipoConsulta T ON  T.Id = C.TipoConsultaId
	WHERE P.Estatus = 1 AND C.Estatus = 1 AND T.Id = @TipoConsultaId
	--	  AND C.FechaConsulta between DATEADD(dd,DATEDIFF(dd,28,GETDATE()),0)	--Obtine la fecha de 28 dias atras
--								AND GETDATE()									--Obtiene la fecha Actual
	ORDER BY C.FechaConsulta  DESC
END


GO
/****** Object:  StoredProcedure [dbo].[GetAllConsultaCalendario]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetAllConsultaCalendario]
AS
BEGIN

	SELECT C.*, P.Nombre, P.ApellidoPaterno, P.ApellidoMaterno, TC.Nombre AS ConsultaNombre, TC.Duracion
	FROM Consulta AS C
		INNER JOIN Paciente AS P ON C.PacienteID = P.Id
		INNER JOIN TipoConsulta AS TC ON C.TipoConsultaID = TC.Id
	WHERE C.Estatus = 1 AND P.Estatus = 1 

END
GO
/****** Object:  StoredProcedure [dbo].[GetAllPacienteByEstatus]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetAllPacienteByEstatus]        
AS              
BEGIN              
 SELECT P.[Id], P.[Nombre], P.[ApellidoPaterno], P.[ApellidoMaterno],
		P.[Nombre] + ' ' + P.ApellidoPaterno + ' ' + P.ApellidoMaterno AS NombreCompleto,
		((cast(datediff(dd,P.FechaNacimiento,GETDATE()) / 365.25 as int)))
		 as Edad, P.[Sexo], P.[Email], P.[FechaAlta], P.[TelefonoCel], 
		P.Estatus, P.Comentarios, p.FechaNacimiento
 FROM Paciente P                              
 WHERE P.Estatus = 1
 ORDER BY P.Id DESC
END
GO
/****** Object:  StoredProcedure [dbo].[GetAllPacientes]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetAllPacientes]
(
	@CompaniaID	INT
)
AS
BEGIN

	SELECT *
	FROM Paciente AS P
		INNER JOIN PermisosPorCompania(@CompaniaID) AS PP ON P.CreatedBY = PP.UserName
	WHERE P.Estatus = 1

END
GO
/****** Object:  StoredProcedure [dbo].[GetAllPagoPaqueteByPacientePaqueteIdDT]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =================================================================================================================
-- Author:		Byron David Rodriguez Herrera
-- Create date: 22/06/2015
-- Description:	Regresa Precio, Abono y Adeduo del Paquete Seleccionado
-- =================================================================================================================
CREATE PROCEDURE [dbo].[GetAllPagoPaqueteByPacientePaqueteIdDT]
(
	@pacientepadoID INT
)
AS
BEGIN
Select P.Costo as Precio, sum(PGP.Pago) as Pagado,  P.Costo - sum(PGP.Pago) as Adeuda from Paquete P
	inner join pacientepaquete PP on P.Id = PP.PaqueteID
	inner Join PagoPaquete PGP on PP.id = PGP.PacientePaqueteID
where PP.Id = @pacientepadoID
group By P.Costo
END
GO
/****** Object:  StoredProcedure [dbo].[GetAllPaqueteDT]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--=================================================================================================================            
-- Author:  Byron David Rodriguez Herrera            
-- Create date: 17/06/2015            
-- Description: Regresa un listado con todos los Paquetes dados de alta.
-- =================================================================================================================            
CREATE PROCEDURE [dbo].[GetAllPaqueteDT]
AS
BEGIN
	SELECT P.Id, P.Nombre, P.Descripcion, P.FechaCreacion, P.Estatus, P.Costo
	FROM Paquete AS P
	WHERE P.Estatus = 1
	ORDER BY P.id desc
END
GO
/****** Object:  StoredProcedure [dbo].[GetAllPaquetes]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetAllPaquetes]
AS
BEGIN

	SELECT *
	FROM Paquete
	WHERE Estatus = 1

END
GO
/****** Object:  StoredProcedure [dbo].[GetAllTipoConsulta]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetAllTipoConsulta]
AS
BEGIN
	
	SELECT *
	FROM TipoConsulta
	ORDER BY Nombre

END
GO
/****** Object:  StoredProcedure [dbo].[GetAllTratamiendoByPaqueteID]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =================================================================================================================            
-- Author:  Byron David Rodriguez Herrera            
-- Create date: 17/06/2015            
-- Description: Regresa un listado con todos los Tratamientos en un paquete.
-- =================================================================================================================            
CREATE PROCEDURE [dbo].[GetAllTratamiendoByPaqueteID]
(
	@paqueteID INT
)
AS
BEGIN
	SELECT --TP.ID,TP.PaqueteID, T.Duracion,
			TP.TratamientoID AS ID, T.Nombre FROM TRATAMIENTOPAQUETE TP
		INNER JOIN TRATAMIENTO T ON T.ID = TP.TRATAMIENTOID
	WHERE TP.PAQUETEID = @paqueteID
	ORDER BY T.Nombre
END


GO
/****** Object:  StoredProcedure [dbo].[GetAllTratamiento]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetAllTratamiento]
AS
BEGIN

	SELECT *
	FROM Tratamiento
	WHERE Estatus = 1
	ORDER BY Nombre

END
GO
/****** Object:  StoredProcedure [dbo].[GetCitaByFechaCita]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCitaByFechaCita]
(
@fechaCita Datetime
)
AS 
BEGIN
SELECT  C.Id,
		C.Estatus,
		C.FechaCita,
		C.PacienteId,
		C.Comentarios,
		C.TratamientoId
FROM Cita C
where DATEPART(DD, FechaCita) = DATEPART(DD, @fechaCita) and
	  DATEPART(mm, FechaCita) = DATEPART(mm, @fechaCita) and
	  DATEPART (hh, FechaCita) = DATEPART(hh, @fechaCita) and
	  DATEPART (mi, FechaCita) = DATEPART(mi, @fechaCita) and
	  C.Estatus = 1			
END
GO
/****** Object:  StoredProcedure [dbo].[GetCitaByPaciente]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCitaByPaciente]
(
	@PacienteID	INT
)
AS
BEGIN

	SELECT C.*, P.Nombre, P.ApellidoPaterno, P.ApellidoMaterno, T.Nombre AS TratamientoNombre
	FROM Cita AS C
		INNER JOIN Paciente AS P ON C.PacienteID = P.Id
		INNER JOIN Tratamiento AS T ON C.TratamientoID = T.Id
	WHERE C.PacienteID = @PacienteID AND C.Estatus = 1 and P.Estatus = 1

END
GO
/****** Object:  StoredProcedure [dbo].[GetCitabyPacienteIDandFechaActual]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCitabyPacienteIDandFechaActual]
(
@IdPaciente int
)
AS
BEGIN
SELECT 
		 C.Id
		,C.Estatus
		,C.FechaCita
		,C.PacienteID
		,C.Comentarios
		,C.TratamientoID
		--,C.TratamientoID
		--T.Nombre
FROM Cita C
	INNER JOIN Paciente P ON P.Id = C.PacienteID
WHERE C.PacienteID = @IdPaciente and 
		DATEPART(DD, c.FechaCita) = DATEPART(DD, GETDATE()) and
		DATEPART(MM, c.FechaCita) = DATEPART(MM, GETDATE()) and
		DATEPART(YY, c.FechaCita) = DATEPART(YY, GETDATE())
END
GO
/****** Object:  StoredProcedure [dbo].[GetCitaTratamientoById]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCitaTratamientoById]
(
	@Id		INT
)
AS
BEGIN

	SELECT C.*, P.Id AS PacienteID, P.Nombre, P.ApellidoPaterno, P.ApellidoMaterno,
		T.Nombre AS TratamientoNombre
	FROM Cita AS C
		INNER JOIN Paciente AS P ON C.PacienteID = P.Id
		INNER JOIN Tratamiento AS T ON C.TratamientoID = T.Id
	WHERE C.Id = @Id

END
GO
/****** Object:  StoredProcedure [dbo].[GetConsultaByFechaConsulta]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetConsultaByFechaConsulta]
(
@fechaConsulta Datetime
)
AS 
BEGIN
--DECLARE @fechaConsulta datetime = '2015-27-04 00:30:00'
SELECT  C.Id,
		C.Estatus,
		C.FechaConsulta,
		C.PacienteId,
		C.Descripcion,
		C.TratamientoId,
		C.CitaID,
		C.TipoConsultaID,
		DATEADD(minute,TC.Duracion,C.FechaConsulta) as FechaFin
FROM Consulta C
		inner join TipoConsulta TC on C.TipoConsultaId = TC.Id
where 
	  DATEPART(DD, C.FechaConsulta) = DATEPART(DD, @fechaConsulta) and
	  DATEPART(mm, C.FechaConsulta) = DATEPART(mm, @fechaConsulta) and
	  DATEPART (hh, C.FechaConsulta) = DATEPART(hh, @fechaConsulta) and
	  DATEPART (mi, C.FechaConsulta) = DATEPART(mi, @fechaConsulta)	and
	  C.Estatus = 1		  		
END


GO
/****** Object:  StoredProcedure [dbo].[GetConsultaByPaciente]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetConsultaByPaciente]
(
	@PacienteID	INT
)
AS
BEGIN

	SELECT C.*, TC.Nombre AS TipoConsultaNombre
	FROM Consulta AS C
		INNER JOIN TipoConsulta  AS TC ON C.TipoConsultaID = TC.Id
	WHERE C.PacienteID = @PacienteID

END
GO
/****** Object:  StoredProcedure [dbo].[GetConsultaCalendarioByIdConsulta]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetConsultaCalendarioByIdConsulta]
(
	@Id		INT
)
AS
BEGIN

	SELECT C.*, P.Nombre, P.ApellidoPaterno, P.ApellidoMaterno, TC.Nombre AS ConsultaNombre, TC.Duracion
	FROM Consulta AS C
		INNER JOIN Paciente AS P ON C.PacienteID = P.Id
		INNER JOIN TipoConsulta AS TC ON C.TipoConsultaID = TC.Id
	WHERE C.Id = @Id

END
GO
/****** Object:  StoredProcedure [dbo].[GetConsultaMayor]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[GetConsultaMayor]
AS
BEGIN
select top(1)	
		CO.Id,	
		CO.Estatus,
		CO.FechaConsulta,
		CO.Descripcion,
		CO.PacienteID
		--CO.TratamientoID,
		--CO.CitaID
from Consulta CO
		INNER JOIN Paciente P ON P.Id = CO.PacienteID
		AND CO.ESTATUS = 1 AND P.ESTATUS = 1
order by CO.Id desc
END
GO
/****** Object:  StoredProcedure [dbo].[GetDatosInstitucion]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetDatosInstitucion]
(
	@Id		INT
)
AS
BEGIN

	SELECT *
	FROM DatosInstitucion
	WHERE Id = @Id

END
GO
/****** Object:  StoredProcedure [dbo].[GetPacienteByFechaNacimiento]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[GetPacienteByFechaNacimiento]
as
begin
Select P.Nombre, P.ApellidoPaterno , P.ApellidoMaterno,
		(DATEPART(YEAR,GETDATE()) - DATEPART(YEAR,FechaNacimiento)) as Edad,
		p.TelefonoCel as Telefono, 
		P.Email as CorreoElectronico
from Paciente P
Where datepart(DD, FechaNacimiento) = datepart(DD, getdate()) and
		datepart(mm, FechaNacimiento) = datepart(mm, getdate())
		and p.Estatus = 1
end
GO
/****** Object:  StoredProcedure [dbo].[GetPacienteById]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPacienteById]  
(  
 @Id  INT,  
 @CompaniaID INT  
)  
AS  
BEGIN  
  
 SELECT *  
 FROM Paciente AS P  
  --INNER JOIN PermisosPorCompania(@CompaniaID) AS PP ON P.CreatedBY = PP.UserName  
 WHERE Id = @Id  
  
END  
GO
/****** Object:  StoredProcedure [dbo].[GetPacientesFiltrados]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPacientesFiltrados]
(
	@Nombre		NVARCHAR(50) = null,
	@APaterno	NVARCHAR(50) = null,
	@AMaterno	NVARCHAR(50) = null,
	@CompaniaID	INT
)
AS
BEGIN

	SELECT *
	FROM Paciente AS P
		--INNER JOIN PermisosPorCompania(@CompaniaID) AS PP ON P.CreatedBy = PP.UserName
	WHERE (Nombre LIKE '%' + @Nombre + '%' OR ApellidoPaterno LIKE '%' + @APaterno + '%' OR ApellidoMaterno LIKE '%' + @AMaterno + '%') AND Estatus = 1

END
GO
/****** Object:  StoredProcedure [dbo].[GetPaqueteById]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPaqueteById]
(
	@Id		INT
)
AS
BEGIN

	SELECT *
	FROM Paquete
	WHERE Id = @Id

END
GO
/****** Object:  StoredProcedure [dbo].[GetRecetaByConsulta]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =================================================================================================================            
-- Author:  Ivan Francisco Bartolo Castro            
-- Create date: 13/05/2015            
-- Description: Regresa un listado de pacientes, con sus datos basicos para armar el reporte de la receta
-- =================================================================================================================            
CREATE PROCEDURE [dbo].[GetRecetaByConsulta]
(
	 @consultaID	INT
)
AS
BEGIN
	SELECT P.Nombre + ' ' + P.ApellidoPaterno +  ' ' + P.ApellidoMaterno AS Paciente,
			C.FechaConsulta, 
			ISNULL(C.Descripcion,'') AS Consulta, 
			ISNULL(C.DescripcionReceta,'') AS Medicamento,
			ISNULL(T.Nombre,'') AS TipoConsulta,
			C.FechaProximaConsulta AS ProximaConsulta
	FROM Paciente AS P
		INNER JOIN Consulta AS C ON P.Id = C.PacienteID
		LEFT OUTER JOIN TipoConsulta AS T ON C.TipoConsultaID = T.Id
	WHERE C.Id = @consultaID
END
GO
/****** Object:  StoredProcedure [dbo].[GetTipoConsultaById]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetTipoConsultaById]
(
	@Id		INT
)
AS
BEGIN

	SELECT *
	FROM TipoConsulta
	WHERE Id = @Id

END
GO
/****** Object:  StoredProcedure [dbo].[GetTratamientoById]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetTratamientoById]
(
	@Id		INT
)
AS
BEGIN

	SELECT *
	FROM Tratamiento
	WHERE Id = @Id

END
GO
/****** Object:  StoredProcedure [dbo].[GetTratamientoPorPauqte]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetTratamientoPorPauqte]
(
	@Id		INT
)
AS
BEGIN

	SELECT *
	FROM Tratamiento
	WHERE Id IN
	(	
		SELECT TratamientoID
		FROM TratamientoPaquete AS TP
		WHERE TP.PaqueteID = @Id
	)

END
GO
/****** Object:  StoredProcedure [dbo].[SaveOrUpdateCita]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SaveOrUpdateCita]
(
	@Id int,
	@Estatus bit,
	@FechaCita datetime = NULL,
	@Paciente int,
	@Comentarios nvarchar(500) = NULL,
	@Tratamiento int = NULL,
	@Estado int = NULL
)
AS
BEGIN

	IF EXISTS(SELECT * FROM Cita WHERE Id = @Id)
		BEGIN
			UPDATE Cita
				SET Estatus = @Estado, FechaCita = @FechaCita, PacienteID = @Paciente, Comentarios = @Comentarios,
				TratamientoID = @Tratamiento, Estado = @Estado
			WHERE Id = @Id
			SELECT @Id
		END
	ELSE	
		BEGIN
			INSERT INTO Cita
			(Estatus, FechaCita, PacienteID, Comentarios, TratamientoID, Estado)
			VALUES
			(@Estatus, @FechaCita, @Paciente, @Comentarios, @Tratamiento, @Estado)
			SELECT @@IDENTITY
		END

END
GO
/****** Object:  StoredProcedure [dbo].[SaveOrUpdateConsulta]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SaveOrUpdateConsulta]
(
	@Id int,
	@Estatus bit ,
	@FechaConsulta datetime = NULL,
	@Paciente int ,
	@Descripcion nvarchar(500) = NULL,
	@TipoConsulta int = NULL,
	@DescripcionReceta nvarchar(1000) = NULL,
	@Comentarios nvarchar(500) = NULL,
	@FechaProximaConsulta datetime = NULL
)
AS
BEGIN

	IF EXISTS(SELECT * FROM Consulta WHERE Id = @Id)
		BEGIN

			UPDATE Consulta
				SET Estatus = @Estatus, FechaConsulta = @FechaConsulta, PacienteID = @Paciente, Descripcion = @Descripcion, 
				TipoConsultaID = @TipoConsulta, DescripcionReceta = @DescripcionReceta, Comentarios = @Comentarios,
				FechaProximaConsulta = @FechaConsulta
			WHERE Id = @Id
			SELECT @Id

		END
	ELSE
		BEGIN

			INSERT INTO Consulta
			(Estatus, FechaConsulta, PacienteID, Descripcion, TipoConsultaID, DescripcionReceta, Comentarios, FechaProximaConsulta)
			VALUES
			(@Estatus, @FechaConsulta, @Paciente, @Descripcion, @TipoConsulta, @DescripcionReceta, @Comentarios, @FechaProximaConsulta)
			SELECT @@IDENTITY

		END

END
GO
/****** Object:  StoredProcedure [dbo].[SaveOrUpdateDatosInstitucion]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SaveOrUpdateDatosInstitucion]
(
	@Id int,
	@Nombre nvarchar(100) = NULL,
	@NoTrabajadores int = NULL,
	@Telefono1 nvarchar(50) = NULL,
	@Telefono2 nvarchar(50) = NULL,
	@Email nvarchar(100) = NULL,
	@Calle nvarchar(50) = NULL,
	@NoInterior nvarchar(10) = NULL,
	@NoExterior nvarchar(10) = NULL,
	@Colonia nvarchar(50) = NULL,
	@Deleacion nvarchar(50) = NULL,
	@Municipio nvarchar(50) = NULL,
	@Estado nvarchar(50) = NULL,
	@CP int = NULL,
	@Pais nvarchar(50) = NULL
)
AS
BEGIN

	IF EXISTS(SELECT * FROM DatosInstitucion WHERE Id = @Id)
		BEGIN

			UPDATE DatosInstitucion
				SET Nombre = @Nombre, NoTrabajadores = @NoTrabajadores, Telefono1 = @Telefono1, Telefono2 = @Telefono2, Email = @Email, Calle = @Calle,
				NoInterior = @NoInterior, NoExterior = @NoExterior, Colonia = @Colonia, Deleacion = @Deleacion, Municipio = @Municipio, Estado = @Estado,
				CP = @CP, Pais = @Pais
			WHERE Id = @Id
			SELECT @Id
			
		END
	ELSE
		BEGIN

			INSERT INTO DatosInstitucion
			(Nombre, NoTrabajadores, Telefono1, Telefono2, Email, Calle, NoInterior, NoExterior, Colonia, Deleacion, Municipio, Estado, CP, Pais)
			VALUES
			(@Nombre, @NoTrabajadores, @Telefono1, @Telefono2, @Email, @Calle, @NoInterior, @NoExterior, @Colonia, @Deleacion, @Municipio, @Estado, @CP, @Pais)
			SELECT @@IDENTITY

		END

END
GO
/****** Object:  StoredProcedure [dbo].[SaveOrUpdatePaciente]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SaveOrUpdatePaciente]
(
	@Id int,
	@Nombre nvarchar(50) = NULL,
	@ApellidoPaterno nvarchar(50) = NULL,
	@ApellidoMaterno nvarchar(50) = NULL,
	@Email nvarchar(50) = NULL,
	@FechaAlta datetime = NULL,
	@TelefonoCel nvarchar(100) = NULL,
	@Comentarios nvarchar(1000) = NULL,
	@Estatus bit = NULL,
	@Edad int = NULL,
	@Sexo nvarchar(15) = NULL,
	@FechaNacimiento datetime = NULL
)
AS
BEGIN

	IF EXISTS(SELECT * FROM Paciente WHERE Id = @Id)
		BEGIN

			update Paciente
				SET Nombre = @Nombre, ApellidoPaterno = @ApellidoPaterno, ApellidoMaterno = @ApellidoMaterno, Email = @Email, FechaAlta = @FechaAlta, TelefonoCel = @TelefonoCel, Comentarios = @Comentarios, Estatus = @Estatus,
				Edad = @Edad, Sexo = @Sexo, FechaNacimiento = @FechaNacimiento
			where Id = @Id
			select @Id

		END
	ELSE
		BEGIN

			insert into Paciente
			(Nombre, ApellidoPaterno, ApellidoMaterno, Email, FechaAlta, TelefonoCel, Comentarios, Estatus, Edad, Sexo, FechaNacimiento)
			values
			(@Nombre, @ApellidoPaterno, @ApellidoMaterno, @Email, @FechaAlta, @TelefonoCel, @Comentarios, @Estatus, @Edad, @Sexo, @FechaNacimiento)
			select @@IDENTITY

		END

END
GO
/****** Object:  StoredProcedure [dbo].[SaveOrUpdatePaquete]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SaveOrUpdatePaquete]
(
	@Id int,
	@Nombre nvarchar(100) = NULL,
	@Descripcion nvarchar(500) = NULL,
	@FechaCreacion datetime = NULL,
	@Estatus bit = NULL,
	@Costo int = NULL
)
AS
BEGIN

	IF EXISTS(SELECT * FROM Paquete WHERE Id = @Id)
		BEGIN
			UPDATE Paquete
				SET Nombre = @Nombre, Descripcion = @Descripcion, FechaCreacion = @FechaCreacion, Estatus = @Estatus,
				Costo = @Costo
			WHERE Id = @Id
			SELECT @Id
		END
	ELSE
		BEGIN
			INSERT INTO Paquete
			(Nombre, Descripcion, FechaCreacion, Estatus, Costo)
			VALUES
			(@Nombre, @Descripcion, @FechaCreacion, @Estatus, @Costo)
			SELECT @@IDENTITY
		END

END
GO
/****** Object:  StoredProcedure [dbo].[SaveOrUpdateTratamiento]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SaveOrUpdateTratamiento]
(
	@Id				INT,
	@Nombre			NVARCHAR(50),
	@Duracion		INT,
	@Descripcion	NVARCHAR(500),
	@Estatus		BIT
)
AS
BEGIN

	IF EXISTS(SELECT * FROM Tratamiento WHERE Id = @Id)
		BEGIN
			UPDATE Tratamiento
				SET Nombre = @Nombre, Duracion = @Duracion, Descripcion = @Descripcion, Estatus = @Estatus
			WHERE Id = @Id
			SELECT @Id
		END
	ELSE
		BEGIN
			INSERT INTO Tratamiento
			(Nombre, Duracion, Descripcion, Estatus)
			VALUES
			(@Nombre, @Duracion, @Descripcion, @Estatus)
			SELECT @@IDENTITY
		END

END
GO
/****** Object:  StoredProcedure [dbo].[SaveOrUpdateTratamientoPaquete]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SaveOrUpdateTratamientoPaquete]
(
	@Id				INT,
	@Paquete		INT,
	@Tratamiento	INT
)
AS
BEGIN

	IF NOT EXISTS(SELECT * FROM TratamientoPaquete WHERE PaqueteID = @Paquete AND TratamientoID = @Tratamiento)
		BEGIN

			INSERT INTO TratamientoPaquete
			(PaqueteID, TratamientoID)
			VALUES
			(@Paquete, @Tratamiento)
	
			SELECT @@IDENTITY

		END
	ELSE
		BEGIN

			SELECT Id
			FROM TratamientoPaquete 
			WHERE PaqueteID = @Paquete AND TratamientoID = @Tratamiento

		END

END
GO
/****** Object:  StoredProcedure [dbo].[SaveOrUpdateUsuario]    Script Date: 24/08/2026 08:51:56 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SaveOrUpdateUsuario]
(
	@Id int,
	@UserName nvarchar(50) = NULL,
	@Password nvarchar(50) = NULL,
	@Estatus bit,
	@CompaniaID int = NULL
)
AS
BEGIN

	IF EXISTS(SELECT * FROM Usuario WHERE Id = @Id)
		BEGIN

			UPDATE Usuario
				SET UserName = @UserName, Password = @Password, Estatus = @Estatus, CompaniaID = @CompaniaID
			WHERE Id = @Id
			SELECT @Id

		END
	ELSE
		BEGIN

			INSERT INTO Usuario
			(UserName, Password, Estatus, CompaniaID)
			VALUES
			(@UserName, @Password, @Estatus, @CompaniaID)
			SELECT @@IDENTITY

		END

END

GO
USE [master]
GO
ALTER DATABASE [DB_9C7990_Consultorio] SET  READ_WRITE 
GO
