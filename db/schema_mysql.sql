-- MySQL schema for API-tester (InnoDB, utf8mb4)
CREATE DATABASE IF NOT EXISTS `api_tester` CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci;
USE `api_tester`;

CREATE TABLE `Users` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Username` VARCHAR(100) NOT NULL,
  `Email` VARCHAR(320) NOT NULL,
  `CreatedAt` DATETIME(6) NOT NULL,
  `IsActive` TINYINT(1) NOT NULL,
  `Role` VARCHAR(50) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `Workspaces` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Name` VARCHAR(200) NOT NULL,
  `Description` TEXT NOT NULL,
  `CreatedAt` DATETIME(6) NOT NULL,
  `OwnerUserId` INT NOT NULL,
  PRIMARY KEY (`Id`),
  CONSTRAINT `FK_Workspaces_OwnerUser` FOREIGN KEY (`OwnerUserId`) REFERENCES `Users`(`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `WorkspaceMembership` (
  `UserId` INT NOT NULL,
  `WorkspaceId` INT NOT NULL,
  `MemberRole` VARCHAR(100) NOT NULL,
  `JoinedAt` DATETIME(6) NOT NULL,
  `IsOwner` TINYINT(1) NOT NULL,
  PRIMARY KEY (`UserId`,`WorkspaceId`),
  CONSTRAINT `FK_WSMember_User` FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_WSMember_Workspace` FOREIGN KEY (`WorkspaceId`) REFERENCES `Workspaces`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `Collections` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Name` VARCHAR(200) NOT NULL,
  `Description` TEXT NOT NULL,
  `CreatedAt` DATETIME(6) NOT NULL,
  `IsShared` TINYINT(1) NOT NULL,
  `WorkspaceId` INT NOT NULL,
  PRIMARY KEY (`Id`),
  CONSTRAINT `FK_Collections_Workspace` FOREIGN KEY (`WorkspaceId`) REFERENCES `Workspaces`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `Requests` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Name` VARCHAR(200) NOT NULL,
  `Url` VARCHAR(2000) NOT NULL,
  `Method` INT NOT NULL,
  `Body` LONGTEXT NOT NULL,
  `CreatedAt` DATETIME(6) NOT NULL,
  `LastExecutedAt` DATETIME(6) NULL,
  `CollectionId` INT NOT NULL,
  PRIMARY KEY (`Id`),
  CONSTRAINT `FK_Requests_Collection` FOREIGN KEY (`CollectionId`) REFERENCES `Collections`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `Headers` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `RequestId` INT NOT NULL,
  `Key` VARCHAR(200) NOT NULL,
  `Value` LONGTEXT NOT NULL,
  `IsEnabled` TINYINT(1) NOT NULL,
  PRIMARY KEY (`Id`),
  CONSTRAINT `FK_Headers_Request` FOREIGN KEY (`RequestId`) REFERENCES `Requests`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `Responses` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `RequestId` INT NOT NULL,
  `StatusCode` INT NOT NULL,
  `IsSuccess` TINYINT(1) NOT NULL,
  `ReceivedAt` DATETIME(6) NOT NULL,
  `DurationMs` BIGINT NOT NULL,
  `PayloadSizeBytes` INT NOT NULL,
  `ResponseBody` LONGTEXT NOT NULL,
  PRIMARY KEY (`Id`),
  CONSTRAINT `FK_Responses_Request` FOREIGN KEY (`RequestId`) REFERENCES `Requests`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `Environments` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Name` VARCHAR(200) NOT NULL,
  `Type` INT NOT NULL,
  `BaseUrl` VARCHAR(2000) NOT NULL,
  `IsActive` TINYINT(1) NOT NULL,
  `CreatedAt` DATETIME(6) NOT NULL,
  `WorkspaceId` INT NOT NULL,
  PRIMARY KEY (`Id`),
  CONSTRAINT `FK_Environments_Workspace` FOREIGN KEY (`WorkspaceId`) REFERENCES `Workspaces`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `EnvironmentVariables` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `EnvironmentId` INT NOT NULL,
  `Key` VARCHAR(200) NOT NULL,
  `Value` LONGTEXT NOT NULL,
  `IsSecret` TINYINT(1) NOT NULL,
  `LastUpdatedAt` DATETIME(6) NOT NULL,
  PRIMARY KEY (`Id`),
  CONSTRAINT `FK_EnvVar_Environment` FOREIGN KEY (`EnvironmentId`) REFERENCES `Environments`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `RequestEnvironmentLink` (
  `RequestId` INT NOT NULL,
  `EnvironmentId` INT NOT NULL,
  `LinkedAt` DATETIME(6) NOT NULL,
  `IsDefaultEnvironment` TINYINT(1) NOT NULL,
  PRIMARY KEY (`RequestId`,`EnvironmentId`),
  CONSTRAINT `FK_REQENV_Request` FOREIGN KEY (`RequestId`) REFERENCES `Requests`(`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_REQENV_Environment` FOREIGN KEY (`EnvironmentId`) REFERENCES `Environments`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `RequestTag` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Name` VARCHAR(100) NOT NULL,
  `ColorHex` VARCHAR(7) NOT NULL,
  `CreatedAt` DATETIME(6) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `RequestTagMap` (
  `RequestId` INT NOT NULL,
  `TagId` INT NOT NULL,
  `LinkedAt` DATETIME(6) NOT NULL,
  PRIMARY KEY (`RequestId`,`TagId`),
  CONSTRAINT `FK_TagMap_Request` FOREIGN KEY (`RequestId`) REFERENCES `Requests`(`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_TagMap_Tag` FOREIGN KEY (`TagId`) REFERENCES `RequestTag`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
