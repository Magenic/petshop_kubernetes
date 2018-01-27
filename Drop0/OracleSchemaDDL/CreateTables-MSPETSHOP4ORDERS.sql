CREATE USER MSPETSHOP4ORDERS IDENTIFIED BY "pass@word1" DEFAULT TABLESPACE USERS QUOTA UNLIMITED ON USERS; 

GRANT CREATE SESSION TO MSPETSHOP4ORDERS; 

CREATE TABLE MSPETSHOP4ORDERS.ORDERS (
    OrderId int NOT NULL,
    UserId varchar(80) NOT NULL,
    OrderDate date NOT NULL,
    ShipAddr1 varchar(80) NOT NULL,
    ShipAddr2 varchar(80) NULL,
    ShipCity varchar(80) NOT NULL,
    ShipState varchar(80) NOT NULL,
    ShipZip varchar(20) NOT NULL,
    ShipCountry varchar(20) NOT NULL,
    BillAddr1 varchar(80) NOT NULL,
    BillAddr2 varchar(80)  NULL,
    BillCity varchar(80) NOT NULL,
    BillState varchar(80) NOT NULL,
    BillZip varchar(20) NOT NULL,
    BillCountry varchar(20) NOT NULL,
    Courier varchar(80) NOT NULL,
    TotalPrice number(10,2) NOT NULL,
    BillToFirstName varchar(80) NOT NULL,
    BillToLastName varchar(80) NOT NULL,
    ShipToFirstName varchar(80) NOT NULL,
    ShipToLastName varchar(80) NOT NULL,
    AuthorizationNumber int NOT NULL,
    Locale varchar(20) NOT NULL,
    CONSTRAINT PK_ORDERS PRIMARY KEY (OrderId) );

CREATE TABLE MSPETSHOP4ORDERS.ORDERSTATUS (
    OrderId int NOT NULL,
    LineNum int NOT NULL,
    Timestamp date NOT NULL,
    Status varchar(2) NOT NULL,
    CONSTRAINT PK_ORDERSTATUS PRIMARY KEY (orderid, linenum),
        CONSTRAINT FK_ORDERSTATUS FOREIGN KEY (orderid)
        REFERENCES MSPETSHOP4ORDERS.ORDERS (OrderId) );
        
CREATE TABLE MSPETSHOP4ORDERS.LINEITEM (
    OrderId int NOT NULL,
    LineNum int NOT NULL,
    ItemId varchar(10) NOT NULL,
    Quantity int NOT NULL,
    UnitPrice number(10,2) NOT NULL,
    CONSTRAINT PK_LINEITEM PRIMARY KEY (OrderId, LineNum),
        CONSTRAINT FK_LINEITEM FOREIGN KEY (OrderId)
        REFERENCES MSPETSHOP4ORDERS.ORDERS (OrderId) );
        
CREATE SEQUENCE MSPETSHOP4ORDERS.ORDERNUM INCREMENT BY 1 START WITH 1 MAXVALUE 1.0E27 MINVALUE 1 NOCYCLE CACHE 10000 NOORDER;