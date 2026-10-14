CREATE TABLE `Users` (
  `ID` integer PRIMARY KEY,
  `Login` varchar(100) UNIQUE NOT NULL,
  `PasswordHash` varchar(255) NOT NULL,
  `Role` varchar(20) NOT NULL COMMENT 'Client | Employee | Admin',
  `IsActive` boolean NOT NULL DEFAULT true
);

CREATE TABLE `Clients` (
  `ID` integer PRIMARY KEY,
  `UserID` integer NOT NULL,
  `FirstName` varchar(100) NOT NULL,
  `LastName` varchar(100) NOT NULL,
  `Phone` varchar(30),
  `Email` varchar(150),
  `RegistrationDate` timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP)
);

CREATE TABLE `Employees` (
  `ID` integer PRIMARY KEY,
  `UserID` integer NOT NULL,
  `FirstName` varchar(100) NOT NULL,
  `LastName` varchar(100) NOT NULL,
  `Position` varchar(100),
  `Specialization` varchar(150)
);

CREATE TABLE `Services` (
  `ID` integer PRIMARY KEY,
  `Name` varchar(150) NOT NULL,
  `Description` varchar(500),
  `Price` decimal(10,2) NOT NULL,
  `ServiceType` varchar(20) NOT NULL COMMENT 'Печать | Съемка | Ретушь'
);

CREATE TABLE `Products` (
  `ID` integer PRIMARY KEY,
  `Name` varchar(150) NOT NULL,
  `Description` varchar(500),
  `Price` decimal(10,2) NOT NULL,
  `QuantityInStock` integer NOT NULL DEFAULT 0
);

CREATE TABLE `Suppliers` (
  `ID` integer PRIMARY KEY,
  `Name` varchar(150) NOT NULL,
  `ContactPerson` varchar(100),
  `Phone` varchar(30),
  `Email` varchar(150),
  `Address` varchar(255)
);

CREATE TABLE `Materials` (
  `ID` integer PRIMARY KEY,
  `Name` varchar(150) NOT NULL,
  `Quantity` decimal(10,2) NOT NULL DEFAULT 0,
  `Unit` varchar(30) NOT NULL,
  `MinQuantity` decimal(10,2) NOT NULL DEFAULT 0,
  `LastSupplyDate` date
);

CREATE TABLE `SupplyRequests` (
  `ID` integer PRIMARY KEY,
  `MaterialID` integer NOT NULL,
  `SupplierID` integer NOT NULL,
  `Quantity` decimal(10,2) NOT NULL,
  `RequestDate` timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
  `Status` varchar(20) NOT NULL DEFAULT 'New' COMMENT 'New | Ordered | Delivered | Cancelled'
);

CREATE TABLE `ServiceMaterials` (
  `ID` integer PRIMARY KEY,
  `ServiceID` integer NOT NULL,
  `MaterialID` integer NOT NULL,
  `Quantity` decimal(10,2) NOT NULL
);

CREATE TABLE `Orders` (
  `ID` integer PRIMARY KEY,
  `ClientID` integer NOT NULL,
  `EmployeeID` integer,
  `CreatedDate` timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
  `Status` varchar(20) NOT NULL DEFAULT 'New' COMMENT 'New | InProgress | Ready | Completed | Cancelled',
  `TotalPrice` decimal(10,2) NOT NULL DEFAULT 0
);

CREATE TABLE `OrderDetails` (
  `ID` integer PRIMARY KEY,
  `OrderID` integer NOT NULL,
  `ServiceID` integer,
  `ProductID` integer,
  `Quantity` integer NOT NULL DEFAULT 1,
  `Price` decimal(10,2) NOT NULL
);

CREATE TABLE `Reviews` (
  `ID` integer PRIMARY KEY,
  `ClientID` integer NOT NULL,
  `OrderID` integer NOT NULL,
  `Text` varchar(1000),
  `Rating` tinyint NOT NULL COMMENT '1-5',
  `CreatedDate` timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP)
);

ALTER TABLE `Clients` ADD FOREIGN KEY (`UserID`) REFERENCES `Users` (`ID`);

ALTER TABLE `Employees` ADD FOREIGN KEY (`UserID`) REFERENCES `Users` (`ID`);

ALTER TABLE `Orders` ADD FOREIGN KEY (`ClientID`) REFERENCES `Clients` (`ID`);

ALTER TABLE `Orders` ADD FOREIGN KEY (`EmployeeID`) REFERENCES `Employees` (`ID`);

ALTER TABLE `OrderDetails` ADD FOREIGN KEY (`OrderID`) REFERENCES `Orders` (`ID`);

ALTER TABLE `OrderDetails` ADD FOREIGN KEY (`ServiceID`) REFERENCES `Services` (`ID`);

ALTER TABLE `OrderDetails` ADD FOREIGN KEY (`ProductID`) REFERENCES `Products` (`ID`);

ALTER TABLE `Reviews` ADD FOREIGN KEY (`ClientID`) REFERENCES `Clients` (`ID`);

ALTER TABLE `Reviews` ADD FOREIGN KEY (`OrderID`) REFERENCES `Orders` (`ID`);

ALTER TABLE `SupplyRequests` ADD FOREIGN KEY (`MaterialID`) REFERENCES `Materials` (`ID`);

ALTER TABLE `SupplyRequests` ADD FOREIGN KEY (`SupplierID`) REFERENCES `Suppliers` (`ID`);

ALTER TABLE `ServiceMaterials` ADD FOREIGN KEY (`ServiceID`) REFERENCES `Services` (`ID`);

ALTER TABLE `ServiceMaterials` ADD FOREIGN KEY (`MaterialID`) REFERENCES `Materials` (`ID`);

-- Disable foreign key checks for INSERT
SET FOREIGN_KEY_CHECKS = 0;

INSERT INTO `Users` (`ID`, `Login`, `PasswordHash`, `Role`, `IsActive`)
VALUES
  (1, 'admin', '$2a$11$Q9x1o8w2n0v3o9c5o7q7EOa1n6c1s2b1a2d3f4g5h6i7j8k9l0m1n', 'Admin', TRUE),
  (2, 'employee1', '$2a$11$Q9x1o8w2n0v3o9c5o7q7EOa1n6c1s2b1a2d3f4g5h6i7j8k9l0m1n', 'Employee', TRUE),
  (3, 'client1', '$2a$11$Q9x1o8w2n0v3o9c5o7q7EOa1n6c1s2b1a2d3f4g5h6i7j8k9l0m1n', 'Client', TRUE);
INSERT INTO `Clients` (`ID`, `UserID`, `FirstName`, `LastName`, `Phone`, `Email`)
VALUES
  (1, 3, 'Иван', 'Петров', '+375291112233', 'client1@example.com');
INSERT INTO `Employees` (`ID`, `UserID`, `FirstName`, `LastName`, `Position`, `Specialization`)
VALUES
  (1, 2, 'Анна', 'Сидорова', 'Фотограф', 'Портретная съемка');
INSERT INTO `Services` (`ID`, `Name`, `Description`, `Price`, `ServiceType`)
VALUES
  (1, 'Печать фото 10x15', 'Глянцевая или матовая печать', 5, 'Печать'),
  (2, 'Семейная фотосессия', '1 час, студия', 80, 'Съемка'),
  (3, 'Ретушь портрета', 'Профессиональная ретушь одного кадра', 15, 'Ретушь');
INSERT INTO `Products` (`ID`, `Name`, `Description`, `Price`, `QuantityInStock`)
VALUES
  (1, 'Фотоальбом А4', 'Классический фотоальбом на 50 фото', 25, 40),
  (2, 'Фоторамка 15x21', 'Деревянная фоторамка', 12, 60);
INSERT INTO `Suppliers` (`ID`, `Name`, `ContactPerson`, `Phone`, `Email`, `Address`)
VALUES
  (1, 'ФотоСнаб', 'Сергей Иванов', '+375291234567', 'supply@fotosnab.by', 'г. Минск, ул. Складская, 5'),
  (2, 'ХимТорг', 'Ольга Петрова', '+375297654321', 'info@himtorg.by', 'г. Минск, ул. Химическая, 12');
INSERT INTO `Materials` (`ID`, `Name`, `Quantity`, `Unit`, `MinQuantity`, `LastSupplyDate`)
VALUES
  (1, 'Фотобумага глянцевая 10x15', 500, 'лист', 100, '2026-08-01'),
  (2, 'Тонер для принтера', 4, 'картридж', 2, '2026-07-15');
INSERT INTO `SupplyRequests` (`ID`, `MaterialID`, `SupplierID`, `Quantity`, `Status`)
VALUES
  (1, 1, 1, 500, 'Ordered'),
  (2, 2, 2, 10, 'New');
INSERT INTO `ServiceMaterials` (`ID`, `ServiceID`, `MaterialID`, `Quantity`)
VALUES
  (1, 1, 1, 1),
  (2, 1, 2, 0.01);
INSERT INTO `Orders` (`ID`, `ClientID`, `EmployeeID`, `Status`, `TotalPrice`)
VALUES
  (1, 1, 1, 'InProgress', 95);
INSERT INTO `OrderDetails` (`ID`, `OrderID`, `ServiceID`, `ProductID`, `Quantity`, `Price`)
VALUES
  (1, 1, 2, NULL, 1, 80),
  (2, 1, 3, NULL, 1, 15);

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS = 1;