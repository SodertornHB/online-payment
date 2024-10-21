IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Migration')
BEGIN
	CREATE TABLE [dbo].[Migration](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[ClientVersion] [nvarchar](200) NULL, 
		[DatabaseVersion] [nvarchar](200) NULL, 
		[CreatedOn] DateTime NULL
	 CONSTRAINT [PK_Migration] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
	) ON [PRIMARY]

	
	Insert into Migration (ClientVersion, DatabaseVersion, CreatedOn) values  ('1.0.0','1.0.0', GETDATE())

END

IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Log')
BEGIN
CREATE TABLE [dbo].[Log](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Origin] [nvarchar](2000) NULL,
	[Message] [nvarchar](2000) NULL,
	[LogLevel] [nvarchar](2000) NULL,
	[CreatedOn] [datetime] NULL,
	[Exception] [nvarchar](4000) NULL,
	[Trace] [nvarchar](4000) NULL,
 CONSTRAINT [PK_Log] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]

END


IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Payment')
BEGIN
	CREATE TABLE Payment (
		Id INT IDENTITY(1,1) not null,
		Session NVARCHAR(255) NOT NULL,
		BorrowerNumber INT NOT NULL,
		PatronName NVARCHAR(255) NOT NULL,
		PatronEmail NVARCHAR(255),
		PatronPhoneNumber NVARCHAR(50),
		Amount DECIMAL(18, 2) NOT NULL,
		InitiationDateTime DATETIME2 NOT NULL,
		Status NVARCHAR(50) NOT NULL,
		Description NVARCHAR(500)
		CONSTRAINT [PK_Payment] PRIMARY KEY CLUSTERED 
		(
			[Id] DESC
		)
	) ON [PRIMARY]
END

IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PaymentRequest')
BEGIN
    CREATE TABLE PaymentRequest (
        Id INT IDENTITY(1,1) NOT NULL,
		Session NVARCHAR(255) NOT NULL,
        PayeePaymentReference NVARCHAR(50),
        CallbackUrl NVARCHAR(500),
        PayerAlias NVARCHAR(50),
        PayeeAlias NVARCHAR(50),
        Amount DECIMAL(18, 2) NOT NULL,
        Currency NVARCHAR(10) NOT NULL,
        Message NVARCHAR(255),
		PaymentRequestDateTime DATETIME2 NOT NULL,
        CONSTRAINT [PK_PaymentRequest] PRIMARY KEY CLUSTERED 
        (
            [Id] DESC
        )
    ) ON [PRIMARY]
END


IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PaymentResponse')
BEGIN
    CREATE TABLE PaymentResponse (
        Id INT IDENTITY(1,1) NOT NULL,
        Session NVARCHAR(255) NOT NULL,
        Location NVARCHAR(MAX),
		PaymentResponseReceivedDateTime DATETIME2 NOT NULL,
		CallbackReceivedDateTime DATETIME2 NULL,
        CONSTRAINT [PK_PaymentResponse] PRIMARY KEY CLUSTERED 
        (
            [Id] DESC
        )
    ) ON [PRIMARY]
END

IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PaymentCallback')
BEGIN
    CREATE TABLE PaymentCallback (
        Id INT IDENTITY(1,1) NOT NULL,
        Session NVARCHAR(255) NOT NULL,
        PaymentReference NVARCHAR(50),
        Status NVARCHAR(50),
        Amount DECIMAL(18, 2) NOT NULL,
        Currency NVARCHAR(10) NOT NULL,
        PayerAlias NVARCHAR(50),
        PayeeAlias NVARCHAR(50),
        DatePaid DATETIME2 NOT NULL,
        ErrorCode NVARCHAR(50),
        CONSTRAINT [PK_PaymentCallback] PRIMARY KEY CLUSTERED 
        (
            [Id] DESC
        )
    ) ON [PRIMARY]
END


--IF NOT EXISTS (SELECT 1 FROM Migration WHERE ClientVersion = '1.1.0' AND DatabaseVersion = '1.1.0')
--BEGIN
	
--	Insert into Migration (ClientVersion, DatabaseVersion, CreatedOn) values  ('1.1.0','1.1.0', GETDATE())

--END
