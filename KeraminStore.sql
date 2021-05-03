 create database KeraminStore

 use KeraminStore

 create table Post
 (postCode int not null identity,
 postName varchar(100) not null,
 constraint postNameUniq unique (postName),
 constraint postCodePK primary key (postCode))
 insert into Post(postName) values ('Администратор'), ('Штатный сотрудник')

 create table Employee
 (employeeCode int not null identity,
 employeeLogin varchar(30) not null,
 constraint employeeLoginUniq unique (employeeLogin),
 employeeAdminStatus bit not null,
 employeePassword varchar(30) not null,
 employeeName varchar(50) not null,
 employeeSurname varchar(50) not null,
 employeePatronymic varchar(50) not null,
 employeeDateOfBirth date not null,
 constraint employeeCodePK primary key (employeeCode),
 postCode int not null,
 constraint postCodeFK foreign key (postCode) references Post (postCode) on delete cascade)
 insert into Employee(employeeLogin, employeePassword, employeeName, employeeSurname, employeePatronymic, employeeDateOfBirth, postCode, employeeAdminStatus) values ('Admin', 'qwerty', 'Александр', 'Ритвинский', 'Николаевич', '19.10.2001', 1, 1)

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
 insert into ProductType (productTypeName) values ('Настенная плитка'), ('Напольная плитка'), ('Настенный декор'), ('Напольный декор'), ('Мозаика'), ('Бордюр')

 create table Surface
 (surfaceCode int not null identity,
 surfaceName varchar(100) not null,
 constraint surfaceNameUniq unique (surfaceName),
 constraint surfaceCodePK primary key (surfaceCode))
 insert into Surface (surfaceName) values ('Глянцевая'), ('Матовая')

 create table Color
 (colorCode int not null identity,
 colorName varchar(100) not null,
 constraint colorNameUniq unique (colorName),
 constraint colorCodePK primary key (colorCode))
 insert into Color (colorName) values ('Разноцветный'), ('Светло-бежевый'), ('Светло-серый'), ('Синий'), ('Темно-бежевый'), ('Темно-серый'), ('Серый'), ('Черный'), ('Бежевый'), ('Кориченевый'), ('Зеленый'), ('Голубой'), ('Розовый'), ('Красный'), ('Фиолетовый'), ('Желтый'), ('Оранжевый')

 create table Product 
 (productCode int not null identity,
 productName varchar(100) not null,
 productArticle varchar(11) not null,
 constraint productArticleUniq unique (productArticle),
 productWidth float not null,
 productLenght float not null,
 productBoxWeight float not null,
 productCount int not null,
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
 colorCode int not null,
 constraint productCollectionCodeFK foreign key (productCollectionCode) references ProductCollection (productCollectionCode) on delete cascade,
 constraint availabilityStatusCodeFK foreign key (availabilityStatusCode) references AvailabilityStatus (availabilityStatusCode) on delete cascade,
 constraint productTypeCodeFK foreign key (productTypeCode) references ProductType (productTypeCode) on delete cascade,
 constraint colorCodeFK foreign key (colorCode) references Color (colorCode) on delete cascade,
 constraint surfaceCodeFK foreign key (surfaceCode) references Surface (surfaceCode) on delete cascade)

 create table Basket
 (basketCode int not null identity,
 basketNumber int not null,
 productsCount int not null,
 boxesCount int not null, 
 productsArea float not null,
 productsWeight float not null,
 generalSum float not null,
 paymentStatus bit not null,
 constraint basketNumberPK primary key (basketCode),
 productCode int not null,
 constraint productCodeFK foreign key (productCode) references Product (productCode) on delete cascade)

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
 insert into Payment (paymentType) values ('Наличными'), ('Банковской картой через платежный терминал'), ('Онлайн оплата картой Visa, Master Card или Белкарт'), ('Через систему "Расчет" (ЕРИП)')

 create table Town
 (townCode int not null identity,
 townName varchar(100) not null,
 constraint townNameUniq unique (townName),
 constraint townCodePK primary key (townCode))
 insert into Town (townName) values ('Минск'), ('Витебск'), ('Брест'), ('Гродно'), ('Гомель'), ('Могилев'), ('Барановичи'), ('Бобруйск'), ('Борисов'), ('Горки'), ('Лида'), ('Мозырь'), ('Молодечно'), ('Пинск'), ('Солигорск')

 create table Street
 (streetCode int not null identity,
 streetName varchar(100) not null,
 constraint streetNameUniq unique (streetName),
 constraint streetCodePK primary key (streetCode))
 insert into Street (streetName) values ('Зеленая'), ('Новая'), ('Железнодорожная'), ('Колесникова')

 create table Customer 
 (customerCode int not null identity,
 customerName varchar(50) null,
 customerSurname varchar(50) null,
 customerPatronymic varchar(50) null,
 phone varchar(17) not null,
 mail varchar(40) not null,
 building int null,
 floor_ int null,
 apartment int null,
 legalName varchar(100) null, 
 UTN int null,
 townCode int null,
 streetCode int null,
 orderNumber int null,
 constraint townCodeFK foreign key (townCode) references Town (townCode) on delete cascade,
 constraint streetCodeFK foreign key (streetCode) references Street (streetCode) on delete cascade,
 constraint customerCodePK primary key (customerCode))

 create table Pickup
 (pickupCode int not null identity,
 streetName varchar(100) not null,
 building int null,
 townCode int not null,
 constraint townFK foreign key (townCode) references Town (townCode) on delete cascade,
 constraint pickupCodePK primary key (pickupCode))

 create table CustomerOrder
 (customerOrderCode int not null identity,
 orderNumber int not null,
 issueDate date not null, 
 generalSum float not null,
 deliveryCost float null,
 constraint customerOrderCodePK primary key (customerOrderCode),
 deliveryCode int not null,
 constraint deliveryCodeFK foreign key (deliveryCode) references Delivery (deliveryCode) on delete cascade,
 basketCode int not null,
 constraint basketCodeFK foreign key (basketCode) references Basket (basketCode) on delete cascade,
 customerCode int not null,
 constraint customerCodeFK foreign key (customerCode) references Customer (customerCode) on delete cascade,
 employeeCode int not null,
 constraint employeeCodeFK foreign key (employeeCode) references Employee (employeeCode) on delete cascade,
 paymentCode int not null,
 constraint paymentCodeFK foreign key (paymentCode) references Payment (paymentCode) on delete cascade,
 pickupCode int null,
 constraint pickupCodeFK foreign key (pickupCode) references Pickup (pickupCode) on delete no action)