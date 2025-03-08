Create database HR_Management_System;

CREATE TABLE Contact (
    ID INT PRIMARY KEY identity(1,1),
    Name VARCHAR(100),
    Email VARCHAR(100),
	Subject varchar(100),
    Message text,
    SubmissionDate DATE
);

CREATE TABLE Department (
    ID INT PRIMARY KEY identity(1,1),
    Name VARCHAR(100),
    Description VARCHAR(500),
	Image varchar(255),
);

CREATE TABLE Managers (
    ID INT PRIMARY KEY identity(1,1),
    Name VARCHAR(100),
    Email VARCHAR(100) UNIQUE,
    Password VARCHAR(255),
    ProfileImage VARCHAR(255),
);

CREATE TABLE Employees (
    ID INT PRIMARY KEY identity(1,1),
    Name VARCHAR(100),
    Email VARCHAR(100) UNIQUE,
    Password VARCHAR(255),
    ProfileImage VARCHAR(255),
    Position VARCHAR(50),
     DepartmentID INT,
    ManagerID INT unique,
    FOREIGN KEY (ManagerID) REFERENCES Managers(ID),
    FOREIGN KEY (DepartmentID) REFERENCES Department(ID)
);


CREATE TABLE HR (
    ID INT PRIMARY KEY identity(1,1),
    Name VARCHAR(100),
    Email VARCHAR(100) UNIQUE,
    Password VARCHAR(255),
    ProfileImage VARCHAR(255),
);

CREATE TABLE Request_Leave (
    ID INT PRIMARY KEY identity(1,1),
    EmployeeID INT,
    RequestDate DATE,
    StartDate DATE,
    EndDate DATE,
	RequestName varchar(255),
	RequestDescription  varchar(255),
	StartTime  TIME,
	EndTime  TIME,
    Status varchar(100),
    FOREIGN KEY (EmployeeID) REFERENCES Employees(ID)
);

CREATE TABLE empTask (
    ID INT PRIMARY KEY identity(1,1),
    Name VARCHAR(100),
    Description TEXT,
    Status VARCHAR(100),
    EmployeeID INT,
    AssignedDate DATE,
    DueDate DATE,
    FOREIGN KEY (EmployeeID) REFERENCES Employees(ID)
);


CREATE TABLE Attendance (
    ID INT PRIMARY KEY identity(1,1),
    EmployeeID INT,
    Date DATE,
    PunchInTime TIME,
    PunchOutTime TIME,
    FOREIGN KEY (EmployeeID) REFERENCES Employees(ID)
);


CREATE TABLE Evaluation (
    ID INT PRIMARY KEY identity(1,1),
    EmployeeID INT,
	ManegerID INT,
    EvaluationDate DATE,
    Status varchar(100),
    FOREIGN KEY (EmployeeID) REFERENCES Employees(ID),
    FOREIGN KEY (ManegerID) REFERENCES Managers(ID)
);