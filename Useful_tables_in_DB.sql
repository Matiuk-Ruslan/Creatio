---------------------------------------------------------------------
--------------------------- LIST OF TABLES --------------------------
---------------------------------------------------------------------

------------------------------ LOGGING ------------------------------

SELECT TOP 1* FROM [UsrServiceLog] WITH(NOLOCK)        -- logging table for import/export entities(WebService: [UsrEvaBPMService])
SELECT TOP 1* FROM [UsrIntegrationLogFtp] WITH(NOLOCK) -- logging table for upload for FTP (WebService: [UsrEvaUnloadOrderOnFtp])
SELECT TOP 1* FROM [SqlConsoleLog] WITH(NOLOCK)        -- Журнал SQL консоли


select top 1* from SysOrderLog with(nolock)
select top 1* from SysContactLog with(nolock)
select top 1* from SysCaseLog with(nolock)
select top 1* from SysAccountLog with(nolock)
select top 1* from SysCallLog with(nolock)

--------------------------- BASE ENTITIES ---------------------------

SELECT TOP 1* FROM [Contact] WITH(NOLOCK)               -- Раздел [Контакты]

SELECT TOP 1* FROM [Order] WITH(NOLOCK)                 -- Раздел [Заказы]
SELECT TOP 1* FROM [SupplyPaymentElement] WITH(NOLOCK)  -- Шаг графика поставок и оплат
SELECT TOP 1* FROM [UsrShares] WITH(NOLOCK)             -- Акции по продуктам

SELECT TOP 1* FROM [Case] WITH(NOLOCK)                  -- Раздел [Обращения]
SELECT TOP 1* FROM [Activity] WITH(NOLOCK)              -- Раздел [Активности]
SELECT TOP 1* FROM [Account] WITH(NOLOCK)               -- Раздел [Контрагенты]
SELECT TOP 1* FROM [UsrCallEvaluation] WITH(NOLOCK)     -- Раздел [Оценивание звонка]
SELECT TOP 1* FROM [SmrLoggingRecord] WITH(NOLOCK)      -- Раздел [Заказы], вкладка [История изменений], деталь [Журнал изменений]

SELECT TOP 1* FROM [SysProcessElementToDo] WITH(NOLOCK) -- список задач по бизнес-процессам на CTI-панели
SELECT TOP 1* FROM [SysProcessElementData] WITH(NOLOCK) -- Данные по запущенным бизнес-процесса 

---------------------- DIRECTORY FOR ENTITIES -----------------------
--------------------------- BASE ENTITIES ---------------------------

SELECT * FROM [OrderStatus] WITH(NOLOCK)              -- Справочник [Состояние заказа]

---------------------------------------------------------------------
----------------------- CHECKING ORDER STATUS -----------------------
---------------------------------------------------------------------

SELECT
[ord].[CreatedOn]   AS [Дата создания заказа],
[ord].[Number]      AS [Номер заказа],
[slr].[CreatedOn]   AS [Дата создания состояния],
[slr].[SmrOldValue] AS [Старое значение],
[slr].[SmrNewValue] AS [Новое значение]
FROM      [Order]            AS [ord] WITH(NOLOCK)
LEFT JOIN [SmrLoggingRecord] AS [slr] WITH(NOLOCK) ON slr.[SmrRecordId]=[ord].[Id]
WHERE 
[slr].SmrColumnCaption='Состояние' 
AND [ord].[Number]='994108957-1'

--------------- PROCESSED ORDER STATUSES BY OPERATORS ---------------

DECLARE
            @BeginDate     datetime,
            @EndDate       datetime,
            @TechAccount_1 uniqueidentifier,
            @TechAccount_2 uniqueidentifier,
            @TechAccount_3 uniqueidentifier
		    SET @BeginDate = '2021-10-01 00:00:00' -- введите дату и время 
        SET @EndDate = GETDATE()               -- текущие дата и время
        SET @TechAccount_1 = '410006E1-CA4E-4502-A9EC-E54D922D2C00'  -- Supervizor 
        SET @TechAccount_2 = '4837F05C-1A77-4598-A33B-A856AFAEDA43'  -- Системный аналитик
        SET @TechAccount_3 = '624F602A-406F-4D41-BF5F-826A8C44EEFC'  -- Технический пользователь
SELECT
[slr].[CreatedOn],
[con].[Name],
[ord].[Number],
[ord].[Comment],
[slr].[SmrColumnCaption],
[slr].[SmrOldValue],
[slr].[SmrNewValue]
FROM      [SmrLoggingRecord] AS [slr] WITH(NOLOCK)
LEFT JOIN [Order]            AS [ord] WITH(NOLOCK) ON [ord].[Id]=[slr].[SmrRecordId]
LEFT JOIN [Contact]          AS [con] WITH(NOLOCK) ON [con].[Id]=[slr].[CreatedById]
WHERE 
[slr].[SmrColumnCaption]='Состояние'
AND [slr].[SmrOldValue] = 'Новый'
AND [slr].[CreatedById] NOT IN ( @TechAccount_1, @TechAccount_2, @TechAccount_3 )
AND [slr].[CreatedOn] BETWEEN @BeginDate
                          AND @EndDate
-------------------------------------------------------------------
DECLARE
            @BeginDate     datetime,
            @EndDate       datetime
		SET @BeginDate = '2021-06-01 00:00:00' -- введите дату и время 
        SET @EndDate = GETDATE()               -- текущие дата и время
SELECT
[slr].[CreatedOn],
[ord].[Number],
[slr].[SmrColumnCaption],
[slr].[SmrOldValue],
[slr].[SmrNewValue]
FROM      [SmrLoggingRecord] AS [slr] WITH(NOLOCK)
LEFT JOIN [Order]            AS [ord] WITH(NOLOCK) ON [ord].[Id]=[slr].[SmrRecordId]
WHERE 
[slr].[SmrColumnCaption]='Состояние'
AND [slr].[SmrOldValue] = 'Не оплачен'
AND [slr].[SmrNewValue] = 'Ошибка оплаты'
AND [slr].[CreatedOn] BETWEEN @BeginDate
                          AND @EndDate

----------------------- CHANGE ORDER STATUS -----------------------

DECLARE
            @CurrentStatus  uniqueidentifier, -- id текущего состояния заказа
            @RequiredStatus uniqueidentifier  -- id требуемого состояния заказа
		    SET @CurrentStatus = '29fa66e3-ef69-4feb-a5af-ec1de125a614'  -- Новый
        SET @RequiredStatus = '40de86ee-274d-4098-9b92-9ebdcf83d4fc' -- Подтвержден
SELECT COUNT(*)
FROM [Order] WITH(NOLOCK)
WHERE
[StatusId] = @CurrentStatus
AND [Comment] = ''

UPDATE [Order]
SET [StatusId] = @RequiredStatus
WHERE [StatusId] = @CurrentStatus 
AND [Comment] = ''

SELECT COUNT(*)
FROM [Order] WITH(NOLOCK)
WHERE
[StatusId] = @CurrentStatus
AND [Comment] = ''

---------------------------------------------------------------------

select id, number, statusid, UsrOrderCancelReasonId, UsrOrderFailureDetailedReason from [order] 
where statusid = '29fa66e3-ef69-4feb-a5af-ec1de125a614'
and UsrCustomerEmail = 'test.mg11@gmail.com' or UsrCustomerEmail = 'faked.email@yandex.ua'

update [order]
set
statusid = 'e06b968e-b596-47e6-80f6-9fb2f391d3e3', -- отменен
UsrOrderCancelReasonId = '7c7ad9ff-6a9b-4cd3-bbba-69a4ea748b08', -- Другое (НЕСТАНДАРТНЫЙ СЛУЧАЙ)
UsrOrderFailureDetailedReason = 'Очистка тестовых заказов' 
where statusid = '29fa66e3-ef69-4feb-a5af-ec1de125a614'
and UsrCustomerEmail = 'test.mg11@gmail.com' or UsrCustomerEmail = 'faked.email@yandex.ua'

---------------------------------------------------------------------
----------------------------- CTI PANEL -----------------------------
---------------------------------------------------------------------

-------- Удаление [Задачи по бизнес-процессам] из CTI-панели --------

DECLARE
            @RequiredContact uniqueidentifier  -- id требуемого контакта
        SET @RequiredContact = '7ee6c45f-50ed-40dc-a787-192124fb0bfb' -- Кукота Марина Олеговна
SELECT COUNT(*) 
FROM [SysProcessElementToDo] WITH(NOLOCK)
WHERE [ContactId] = @RequiredContact

DELETE
FROM [SysProcessElementToDo]
WHERE [ContactId] = @RequiredContact

SELECT COUNT(*) 
FROM [SysProcessElementToDo] WITH(NOLOCK)
WHERE [ContactId] = @RequiredContact

---------------------------------------------------------------------
---------------------- CHECKING ACTIVITY.EMAIL ----------------------
---------------------------------------------------------------------

DECLARE
            @Title       nvarchar(500),         -- тема письма
            @CategoryId  uniqueidentifier,      -- категория активности [E-mail]
            @TypeId      uniqueidentifier,      -- тип активности [E-mail]
            @SenderEmail nvarchar(1000),        -- email отправителя
            @SenderId    uniqueidentifier       -- id контакта отправителя [Технический специалист]
        SET @Title       = 'Нові клієнти у WebitelPhone Ева 793%'
        SET @CategoryId  = '8038a396-7825-e011-8165-00155d043204'
        SET @TypeId      = 'e2831dec-cfc0-df11-b00f-001d60e938c6'
        SET @SenderEmail = 'Reminders_Detractors@stores.eva.ua'
        SET @SenderId    = '25e6ad15-b6a4-46d6-b009-6f3dc00955ba'
SELECT 
[Id]                AS [Id], 
[CreatedOn]         AS [Дата создания], 
[Title]             AS [Тема], 
[StartDate]         AS [Дата отправки], 
[Recepient]         AS [Получатель], 
[EmailSendStatusId] AS [Ошибка], 
[ErrorOnSend]       AS [Описание ошибки]
FROM  [Activity] WITH(NOLOCK) 
WHERE [TypeId]             = @TypeId
AND   [ActivityCategoryId] = @CategoryId
AND   [Sender]             = @SenderEmail
AND   [SenderContactId]    = @SenderId
AND   [Title] LIKE @Title 
ORDER BY [CreatedOn] DESC

---------------------------------------------------------------------
------------- Search for a business process in the logging ----------
---------------------------------------------------------------------

SELECT TOP 5* 
FROM VwSysProcessEntity
WHERE EntityDisplayValue = 'SR01880981'

SELECT top 5 * FROM [VwSysProcessLog]
WHERE Id IN (SELECT SysProcessId FROM [VwSysProcessEntity] WHERE EntityDisplayValue = '009567456')

---------------------------------------------------------------------
--------------------- View stored procedure code --------------------
---------------------------------------------------------------------

SELECT
    sch.name+'.'+ob.name AS       [Object], 
    ob.create_date, 
    ob.modify_date, 
    ob.type_desc, 
    mod.definition
FROM 
     sys.objects AS ob
     LEFT JOIN sys.schemas AS sch ON
            sch.schema_id = ob.schema_id
     LEFT JOIN sys.sql_modules AS mod ON
            mod.object_id = ob.object_id
WHERE mod.definition IS NOT NULL

---------------------------------------------------------------------
-------------- View the sizes of tables in the database -------------
---------------------------------------------------------------------

SELECT
t.Name AS TableName,
s.Name AS SchemaName,
p.Rows AS RowCounts,
SUM(a.total_pages) * 8 AS TotalSpaceKB,
SUM(a.used_pages) * 8 AS UsedSpaceKB,
(SUM(a.total_pages) - SUM(a.used_pages)) * 8 AS UnusedSpaceKB
FROM
sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
INNER JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
LEFT OUTER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE
t.Name NOT LIKE 'dt%'
AND t.is_ms_shipped = 0
AND i.object_id > 255
GROUP BY
t.Name, s.Name, p.Rows
ORDER BY
SUM(a.total_pages) * 8;

EXEC sp_columns Activity;

---------------------------------------------------------------------
------------------ Unlocking a third party package ------------------
---------------------------------------------------------------------

DECLARE
            @PackageName  nvarchar(500),     
            @UIdPackage   uniqueidentifier,
            @IsChanged    bit,               
            @IsLocked     bit,               
            @InstallType  int                
            -- Maintainer = 'Customer' 
        SET @PackageName = 'EVA'                                    /*  [Пакет]  */
        SET @UIdPackage  = 'a97f04ff-026b-4330-93ee-4eb7a1b6247c'   /*  [Идентификатор пакета]  */
        SET @IsChanged   = 0                                        /*  0 - пакет разблокирован, 1 - пакет заблокирован (0 - по умолчанию)  */
        SET @IsLocked    = 0                                        /*  0 - пакет разблокирован, 1 - пакет заблокирован (0 - по умолчанию)  */
        SET @InstallType = 1                                        /*  0 - пакет разблокирован, 1 - пакет заблокирован (1 - по умолчанию)  */

-- Чтение
SELECT [Id], [Name], [isChanged], [isLocked], [InstallType], [Maintainer]
FROM [SysPackage] 
WHERE [Name] = @PackageName
  AND [Id]  = @UIdPackage

SELECT [Id], [Name], [Caption], [isChanged], [isLocked], [SysPackageId], [ManagerName]
FROM [SysSchema] 
WHERE [SysPackageId] = (SELECT [Id] 
                        FROM [SysPackage] 
                        WHERE [Name] = @PackageName
                          AND [SysPackageId]  = @UIdPackage)
-- Изменение
UPDATE [SysPackage] 
SET [IsChanged]   = @IsChanged, 
    [IsLocked]    = @IsLocked, 
    [InstallType] = @InstallType
WHERE [Name] = @PackageName
  AND [Id]  = @UIdPackage

UPDATE [SysSchema]
SET [IsChanged] = @IsChanged, 
    [IsLocked]  = @IsLocked
WHERE [SysPackageId] = (SELECT [Id] 
                      FROM [SysPackage] 
                      WHERE [Name] = @PackageName
                        AND [SysPackageId]  = @UIdPackage)
-- Чтение
SELECT [Id], [Name], [isChanged], [isLocked], [InstallType], [Maintainer]
FROM [SysPackage] 
WHERE [Name] = @PackageName
  AND [Id]  = @UIdPackage

SELECT [Id], [Name], [Caption], [isChanged], [isLocked], [SysPackageId], [ManagerName]
FROM [SysSchema] 
WHERE [SysPackageId] = (SELECT [Id] 
                      FROM [SysPackage] 
                      WHERE [Name] = @PackageName 
                        AND [UId]  = @UIdPackage)




--Проблема:

SELECT TOP 10* FROM [OrderProduct] WITH(NOLOCK) WHERE OrderId = '96922958-fa96-4d8f-b00b-b81477da83d0'
SELECT TOP 5* FROM [UsrIntegrationLogFtp] WITH(NOLOCK)
WHERE [UsrName] = 'Выгрузка данных на FTP EVA'
AND [CreatedOn] BETWEEN '2021-11-23 00:00:00' AND '2021-11-23 23:00:00'
AND [UsrDescription] LIKE '%08631177%'
                        FROM [SysPackage] 
                        WHERE [Name] = @PackageName
                          AND [SysPackageId]  = @UIdPackage)

---------------------------------------------------------------------
---------------------- CHECKING ORDER FROM FTP ----------------------
---------------------------------------------------------------------

SELECT TOP 5* FROM [UsrIntegrationLogFtp] WITH(NOLOCK)
WHERE CreatedOn  BETWEEN '2021-11-08 10:00:00' AND '2021-11-08 17:00:00'
AND UsrName = 'Загрузка файлов c папки OUTOrders'
AND UsrDescription LIKE '%08127120%'

---------------------------------------------------------------------
-------------------- Change customer reliability --------------------
---------------------------------------------------------------------

SELECT id, name, MobilePhone, UsrReliabilityId 
from contact 
where MobilePhone in 
(
'380958635004',
'380500482504'
)
order by MobilePhone desc

update contact
set UsrReliabilityId='ed09d511-40cd-49b8-a382-c90fc1866bbc'
where MobilePhone in 
(
'380958635004',
'380500482504'
)

select id, name, MobilePhone, UsrReliabilityId 
from contact 
where MobilePhone in 
(
'380958635004',
'380500482504'
)
order by MobilePhone desc

---------------------------------------------------------------------
----------------------------- Beesender -----------------------------
---------------------------------------------------------------------
select top 5* from BeesenderChatMessage where id = 'fa65e2f9-6b72-45b1-b23c-5086bc6f5c2b'
select top 5* from BeesenderClient where id = '73e1a3f3-90b0-4623-9e02-ae9ce9a82ac3'


SELECT Id, Number FROM [ORDER] WITH(NOLOCK) WHERE Number = '08303025'
SELECT Name, ProductId, UsrSKU, Price FROM [OrderProduct] WITH(NOLOCK) WHERE OrderId = '4f3396d0-0e59-464b-a8da-1dd99747d175'
SELECT Id, Number FROM [ORDER] WITH(NOLOCK) WHERE Number = '08303025-4'
SELECT Name, ProductId, UsrSKU, Price FROM [OrderProduct] WITH(NOLOCK) WHERE OrderId = '751436ea-f3bd-4c5b-8fba-b23e2dd0acb2'

---------------------------------------------------------------------
----------------------- CreatioDB_Task_286549 -----------------------
---------------------------------------------------------------------

SELECT Number, StatusId, UsrUnloadDate FROM [Order] WITH(NOLOCK);
SELECT Id, UsrExecutionDatetime, UsrServiceName, UsrBody FROM UsrOrderIntegrationLog WITH(NOLOCK) ORDER BY UsrExecutionDatetime
DELETE FROM UsrOrderIntegrationLog

UPDATE [Order]
SET Number = '4000020'
WHERE Number = 'ORD-5';
SELECT TOP 1* FROM [Order] WITH(NOLOCK);

UPDATE [Order]
SET UsrUnloadDate = GETDATE()-1
WHERE Number = '4000020';

SELECT Number, StatusId, UsrUnloadDate FROM [Order] WITH(NOLOCK);

sp_help UsrOrderIntegrationLog

select * from NotificationType
select * from Reminding order by CreatedOn
select * from VwSysModuleEntity

---------------------------------------------------------------------
----------------------- CreatioDB_Task_580116 -----------------------
---------------------------------------------------------------------


DELETE FROM [UsrServiceLog]

SELECT [Id]
      ,[UsrRequestTime]
      ,[UsrRequestBody]
      ,[UsrErrors] FROM [UsrServiceLog] WITH(NOLOCK) ORDER BY UsrRequestTime ASC


DELETE FROM [SysOrderLog]

SELECT [Id]
	  ,[ChangeTrackedOn]
      ,[Number]
      ,[Status] FROM [SysOrderLog] WITH(NOLOCK)

---------------------------------------------------------------------
--------------- Доход по заказам с разбивкой по годам ---------------
---------------------------------------------------------------------
SELECT *
FROM
(
    SELECT YEAR(o.CreatedOn) AS [Год заказа]
          ,o.Amount          AS [Стоимость заказа]
    FROM [Order]             AS o
) AS [Заказы]
PIVOT (SUM([Стоимость заказа]) 
FOR [Год заказа] IN ([2019],[2020],[2021])) AS [Итоговая таблица]
GO

SELECT *
FROM
(
    SELECT YEAR(o.CreatedOn)                     AS [Год заказа]
          ,LEFT(DATENAME(MONTH, o.CreatedOn), 3) AS [Месяц заказа]
          ,o.Amount                              AS [Стоимость заказа]
    FROM [Order]                                 AS o
) AS [Заказы]
PIVOT (SUM([Стоимость заказа]) 
FOR [Месяц заказа] IN (Янв, Фев, Мар, Апр, Май, Июн, Июл, Авг, Сен, Окт, Ноя, Дек)) AS [Итоговая таблица]
GO

USE Creatio;
GO

SELECT [Год заказа], 
      ISNULL([Q1], 0) AS Q1, 
      ISNULL([Q2], 0) AS Q2, 
      ISNULL([Q3], 0) AS Q3, 
      ISNULL([Q4], 0) AS Q4, 
      (ISNULL([Q1], 0) + ISNULL([Q2], 0) + ISNULL([Q3], 0) + ISNULL([Q4], 0)) AS [Итог]
FROM
(
    SELECT YEAR(o.CreatedOn) AS [Год заказа]
          ,CAST('Q'+CAST(DATEPART(QUARTER, o.CreatedOn) AS VARCHAR(1)) AS VARCHAR(2)) AS [Квартал]
          ,SUM(L.UnitPrice*L.Quantity) AS [Стоимость заказа]
    FROM [Order] AS o
    GROUP BY YEAR(o.CreatedOn)
            ,CAST('Q'+CAST(DATEPART(QUARTER, o.CreatedOn) AS VARCHAR(1)) AS VARCHAR(2))
) AS [Заказы] 
PIVOT(SUM([Стоимость заказа]) FOR [Квартал] IN([Q1], [Q2], [Q3], [Q4])) AS [Итоговая таблица]
ORDER BY [Год заказа]
GO



---------------------------------------------------------------------
-------------- Очистка дублей из ContactCommunication ---------------
---------------------------------------------------------------------

DECLARE @BeginDate DATETIME = '20220602 00:00:00', 
        @EndDate DATETIME = '20220604 00:00:00', 
        @UsrRequestBody nvarchar(500), 
        @Guid uniqueidentifier 
SELECT TOP (1) @UsrRequestBody = UsrRequestBody 
FROM [UsrServiceLog] WITH(NOLOCK) 
WHERE [UsrRequestBody] LIKE '%данные не записаны%' 
  AND [UsrRequestBody] LIKE '%dbo.ContactCommunication%' 
  AND [CreatedOn] BETWEEN @BeginDate AND @EndDate 
ORDER BY [CreatedOn] 

Select @UsrRequestBody 
DELETE FROM [UsrServiceLog] WHERE [UsrRequestBody] = @UsrRequestBody 

Select @Guid = SUBSTRING(@UsrRequestBody, 10, 36) 
Select @Guid 
SELECT TOP 10* FROM [ContactCommunication] WITH(NOLOCK) WHERE [ContactId] = @Guid 

DELETE TOP (1) FROM [ContactCommunication] 
WHERE [ContactId] = @Guid AND [SearchNumber] NOT LIKE '%@%' 

SELECT TOP 10* FROM [ContactCommunication] WITH(NOLOCK) WHERE [ContactId] = @Guid	

---------------------------------------------------------------------
-------------- Удаление дублей из ContactCommunication --------------
---------------------------------------------------------------------

DECLARE @count int = '1'
WHILE @count < 5
BEGIN
SET @count = @count + 1;

DECLARE @Guid uniqueidentifier = null,
        @ContactId uniqueidentifier = null

SELECT DISTINCT TOP (1) @Guid = CC1.[Id], @ContactId = CC1.[ContactId]
FROM [ContactCommunication] AS CC1 WITH(NOLOCK)
LEFT OUTER JOIN [ContactCommunication] AS CC2 WITH(NOLOCK) ON CC2.Number = CC1.Number
WHERE CC1.[CommunicationTypeId] = 'D4A2DC80-30CA-DF11-9B2A-001D60E938C6'
  AND CC1.[Id] != CC2.[Id]
  AND CC1.[ContactId] = CC2.[ContactId]

SELECT @Guid AS [ContactCommunicationId], @ContactId AS [ContactId]
SELECT [Id], [CommunicationTypeId], [ContactId], [Number] FROM [ContactCommunication] WITH(NOLOCK) WHERE [ContactId] = @ContactId AND [Number] NOT LIKE '%@%' 

DELETE TOP (1) FROM [ContactCommunication] WHERE [ContactId] = @ContactId AND [SearchNumber] NOT LIKE '%@%' 
SELECT [Id], [CommunicationTypeId], [ContactId], [Number] FROM [ContactCommunication] WITH(NOLOCK) WHERE [ContactId] = @ContactId AND [Number] NOT LIKE '%@%' 
END

---------------------------------------------------------------------
-------------------------- Проверка лога ----------------------------
---------------------------------------------------------------------

DECLARE @BeginDate DATETIME = '20220602 00:00:00',
        @EndDate DATETIME = '20220603 00:00:00'
SELECT TOP (1000) 
       UsrRequestTime
	  ,UsrName
	  ,UsrRequestBody
	  ,UsrErrors
FROM  [UsrServiceLog] WITH(NOLOCK)
WHERE [UsrRequestBody] NOT LIKE '%Данные по заказу:%' 
  AND [UsrRequestBody] NOT LIKE '%Заказ был загружен ранее%'
  AND [UsrRequestBody] NOT LIKE '%Заказ не найден%'
  AND [UsrRequestBody] NOT LIKE '%Заказ не загружен%'
  AND [UsrRequestBody] NOT LIKE '%Данные записаны в%'
  AND [UsrRequestBody] NOT LIKE '%контакт обновлен%'
  AND [UsrRequestBody] NOT LIKE '%Заказ обновлен%'
  AND [UsrRequestBody] NOT LIKE '%создан новый контакт%'
  AND [UsrRequestBody] NOT LIKE '%Данные не записаны в%'
  AND [UsrRequestBody] NOT LIKE '%CheckLang%'
  AND [UsrRequestBody] NOT LIKE '%Authorization%'
  AND [UsrRequestBody] NOT LIKE '%Search%'
  AND [UsrRequestBody] NOT LIKE '%GetInfo%'
  AND [UsrRequestBody] NOT LIKE '%GetCardInfo%'
  AND [UsrRequestBody] NOT LIKE '%CardTypeInfo%'
  AND [UsrRequestBody] NOT LIKE '%GetCabinetInfo%'
  AND [UsrRequestBody] NOT LIKE '%CabinetInfoCount%'
  AND [UsrRequestBody] NOT LIKE '%SaveContact%'
  AND [UsrRequestBody] NOT LIKE '%GetTransactions%'
  AND [UsrRequestBody] NOT LIKE '%http://sd.eva.ua/sdpapi%'
  AND [UsrRequestBody] NOT LIKE '%ServiceDesk%'
  AND [UsrRequestBody] NOT LIKE '%CreateActivity%'
  AND [UsrRequestBody] NOT LIKE '%Предыдущий заказ пересобран%'
  AND [UsrRequestBody] NOT LIKE '%CreateContactForRegisterCallBack%'
  AND [UsrRequestBody] NOT LIKE '%{"phone_number%'
  AND [UsrRequestBody] NOT LIKE '%RegisterCaseRequest%'
  AND [UsrRequestBody] NOT LIKE '%по методу оплаты%'
  AND [UsrRequestBody] NOT LIKE '%CreateContact%'
  AND [UsrRequestBody] NOT LIKE '%CreateCity%'
  AND [UsrRequestBody] NOT LIKE '%При создании ActivityId%'
  AND [UsrRequestBody] NOT LIKE '%До обновления ActivityId%'
  AND [UsrRequestBody] NOT LIKE '%После обновления ActivityId%'
  AND [UsrRequestBody] NOT LIKE '%OrderUpdate%'
  AND [CreatedOn] BETWEEN @BeginDate AND @EndDate
ORDER BY [CreatedOn]

---------------------------------------------------------------------
-------------------- Подчинить мастер заказов -----------------------
---------------------------------------------------------------------

delete from  SysModuleEdit  where Id = '73075720-8731-443b-a348-bd715e4eb620'