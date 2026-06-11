USE api_tester;

ALTER TABLE users
ADD COLUMN OIB varchar(11) NOT NULL DEFAULT '00000000000',
ADD COLUMN JMBG varchar(13) NOT NULL DEFAULT '0000000000000';

UPDATE users
SET OIB = '00000000000'
WHERE OIB IS NULL OR OIB = '';

UPDATE users
SET JMBG = '0000000000000'
WHERE JMBG IS NULL OR JMBG = '';

CREATE TABLE IF NOT EXISTS requestattachments (
    Id int NOT NULL AUTO_INCREMENT,
    RequestId int NOT NULL,
    FileName varchar(260) NOT NULL,
    StoredFileName varchar(260) NOT NULL,
    FilePath varchar(1000) NOT NULL,
    ContentType varchar(200) NOT NULL DEFAULT '',
    FileSize bigint NOT NULL,
    CreatedAt datetime(6) NOT NULL,
    CONSTRAINT PK_requestattachments PRIMARY KEY (Id),
    CONSTRAINT FK_requestattachments_requests_RequestId
        FOREIGN KEY (RequestId) REFERENCES requests (Id)
        ON DELETE CASCADE
);

CREATE INDEX IX_requestattachments_RequestId ON requestattachments (RequestId);
