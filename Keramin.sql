 create database KeraminStore

 use KeraminStore

 create table Post
 (postCode int not null identity,
 postName varchar(100) not null,
 constraint postNameUniq unique (postName),
 constraint postCodePK primary key (postCode))
 insert into Post(postName) values ('Администратор'), ('Штатный сотрудник')

 create table Education
 (educationCode int not null identity,
 educationName varchar(100) not null,
 constraint educationNameUniq unique (educationName),
 constraint educationCodePK primary key (educationCode))
 insert into Education(educationName) values ('Среднее образование'), ('Высшее образование'), ('Средне-специальное образование')

 create table Employee
 (employeeCode int not null identity,
 employeeLogin varchar(30) not null,
 constraint employeeLoginUniq unique (employeeLogin),
 employeeAdminStatus bit not null,
 employeePassword varchar(30) not null,
 employeeName varchar(50) not null,
 employeeSurname varchar(50) not null,
 employeePatronymic varchar(50) not null,
 employeePasportNumber varchar(9) not null,
 employeeDateOfBirth date not null,
 constraint employeeCodePK primary key (employeeCode),
 postCode int not null,
 constraint postCodeFK foreign key (postCode) references Post (postCode) on delete cascade,
 educationCode int not null,
 constraint educationCodeFK foreign key (educationCode) references Education (educationCode) on delete cascade)
 insert into Employee(employeeLogin, employeePassword, employeeName, employeeSurname, employeePatronymic, employeePasportNumber, employeeDateOfBirth, postCode, employeeAdminStatus, educationCode) values ('Admin', 'qwerty', 'Александр', 'Ритвинский', 'Николаевич', 'MP3939129', '19.10.2001', 1, 1, 3)

 create table ProductCollection
 (productCollectionCode int not null identity,
 productCollectionName varchar(100) not null,
 constraint productCollectionNameUniq unique (productCollectionName),
 constraint productCollectionCodePK primary key (productCollectionCode))
 insert into ProductCollection (productCollectionName) values ('Классик'), ('Рио'), ('Керкира'), ('Комо'), ('Болонья'), ('Гранада'), ('Букингем'), ('Дюна'), ('Марсала'), ('Метро'), ('Котто'), ('Атрум'), ('Монако'), ('Сидней'), ('Сонора'), ('Телари'), ('Шиен'), ('Сагано'), ('Мари Эрми'), ('Ассам')

 create table AvailabilityStatus
 (availabilityStatusCode int not null identity,
 availabilityStatusName varchar(14),
 constraint availabilityStatusNameUniq unique (availabilityStatusName),
 constraint availabilityStatusCodePK primary key (availabilityStatusCode))
 insert into AvailabilityStatus (availabilityStatusName) values ('Есть в наличии'), ('Нет в наличии')

 create table ProductType
 (productTypeCode int not null identity,
 productTypeName varchar(100) not null,
 constraint productTypeNameUniq unique (productTypeName),
 constraint productTypeCodePK primary key (productTypeCode))
 insert into ProductType (productTypeName) values ('Настенная плитка'), ('Напольная плитка'), ('Настенный декор'), ('Напольный декор'), ('Мозаика'), ('Ступени'), ('Плинтус')

 create table Surface
 (surfaceCode int not null identity,
 surfaceName varchar(100) not null,
 constraint surfaceNameUniq unique (surfaceName),
 constraint surfaceCodePK primary key (surfaceCode))
 insert into Surface (surfaceName) values ('Глянцевая'), ('Матовая'), ('Подполированная'), ('Структурированная'), ('Мозаика'), ('Ступени'), ('Плинтус')

 create table Product 
 (productCode int not null identity,
 productName varchar(100) not null,
 productArticle varchar(11) not null,
 constraint productArticleUniq unique (productArticle),
 productWidth float not null,
 productLenght float not null,
 productBoxWeight float not null,
 productCountInBox int null,
 productCostCount float null,
 productCostArea float null,
 productImage varchar(250) not null,
 productDescription varchar(300) null,
 constraint productCodePK primary key (productCode),
 productCollectionCode int not null, 
 availabilityStatusCode int not null,
 productTypeCode int not null,
 surfaceCode int not null,
 constraint productCollectionCodeFK foreign key (productCollectionCode) references ProductCollection (productCollectionCode) on delete cascade,
 constraint availabilityStatusCodeFK foreign key (availabilityStatusCode) references AvailabilityStatus (availabilityStatusCode) on delete cascade,
 constraint productTypeCodeFK foreign key (productTypeCode) references ProductType (productTypeCode) on delete cascade,
 constraint surfaceCodeFK foreign key (surfaceCode) references Surface (surfaceCode) on delete cascade)

 create table Arrival
 (arrivalCode int not null identity,
 arrivalDate date not null,
 productsCount int not null,
 constraint arrivalCodePK primary key (arrivalCode),
 productCode int not null,
 constraint productCodeArrivalFK foreign key (productCode) references Product (productCode) on delete cascade)

 create table Delivery
 (deliveryCode int not null identity,
 deliveryName varchar(100) not null,
 constraint deliveryNameUniq unique (deliveryName),
 constraint deliveryCodePK primary key (deliveryCode))
 insert into Delivery (deliveryName) values ('Самовывоз'), ('Курьером по г.Минску (Самостоятельно)'), ('Курьером по г.Минску (С подъемом)'), ('Курьером за пределы г.Минска')

 create table Payment
 (paymentCode int not null identity,
 paymentType varchar(100) not null,
 constraint paymentTypeUniq unique (paymentType),
 constraint paymentCodePK primary key (paymentCode))
 insert into Payment (paymentType) values ('Наличными'), ('Банковской картой через платежный терминал'), ('Онлайн оплата картой Visa, Master Card или Белкарт'), ('Через систему "Расчет" (ЕРИП)'), ('Оплата по частям через сервис "ZABIRAY.BY"')

 create table Town
 (townCode int not null identity,
 townName varchar(100) not null,
 constraint townNameUniq unique (townName),
 constraint townCodePK primary key (townCode))

 create table Street
 (streetCode int not null identity,
 streetName varchar(100) not null,
 constraint streetNameUniq unique (streetName),
 constraint streetCodePK primary key (streetCode))

 create table Customer 
 (customerCode int not null identity,
 customerName varchar(50) null,
 customerSurname varchar(50) null,
 customerPhone varchar(17) not null,
 customerMail varchar(40) not null,
 customerBuilding int null,
 customeFloor int null,
 customerApartment int null,
 legalName varchar(100) null, 
 UTN int null,
 townCode int not null,
 streetCode int not null,
 constraint townCodeFK foreign key (townCode) references Town (townCode) on delete cascade,
 constraint streetCodeFK foreign key (streetCode) references Street (streetCode) on delete cascade,
 constraint customerCodePK primary key (customerCode))

 create table CustomerOrder
 (customerOrderCode int not null identity,
 customerOrderNumber varchar(9) not null,
 issueDate date not null, 
 deliveryCost float null,
 constraint customerOrderCodePK primary key (customerOrderCode),
 deliveryCode int not null,
 constraint deliveryCodeFK foreign key (deliveryCode) references Delivery (deliveryCode) on delete cascade,
 customerCode int not null,
 constraint customerCodeFK foreign key (customerCode) references Customer (customerCode) on delete cascade,
 employeeCode int not null,
 constraint employeeCodeFK foreign key (employeeCode) references Employee (employeeCode) on delete cascade,
 paymentCode int not null,
 constraint paymentCodeFK foreign key (paymentCode) references Payment (paymentCode) on delete cascade)

 create table Basket
 (basketCode int not null identity,
 productsCount int not null,
 generalSum float not null,
 constraint basketCodePK primary key (basketCode),
 productCode int not null,
 constraint productCodeFK foreign key (productCode) references Product (productCode) on delete cascade,
 customerOrderCode int not null,
 constraint customerOrderCodeFK foreign key (customerOrderCode) references CustomerOrder (customerOrderCode) on delete cascade)