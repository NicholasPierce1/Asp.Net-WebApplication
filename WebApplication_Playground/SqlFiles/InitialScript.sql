/*
SELECT
  count(*)
FROM
  SYSOBJECTS
WHERE
  xtype = 'U';
  go

  begin transaction

  rollback -- commit
  go

  SELECT * FROM sysusers;

  select CURRENT_USER;
  */
  --create schema Custom;
  use CustomDatabase;
  -- create role AppRole authorization dbo;

  /*
  grant select on schema :: Custom
  to AppRole
  with grant option;
  */
  use CustomDatabase;
  grant 
  alter,control, create sequence, delete, execute, insert, references,
  select, update, view definition
  on schema :: Custom
  to AppRole
  with grant option
  as dbo;

  -- creating user 'App' w/ default schema
  /*
  create login AppLogin
  with password = 'AppLogin';
  create user App for login AppLogin
  with default_schema = Custom; -- optional: specify default schema for all table lookups
  -- add new user to AppRole
  alter role AppRole
  add member App; */

  /*
  indexes:
  clustered- how rows are physically stored and sorted (pks are clustered by default).
             tables only have one cluster index.
  non-clustered: how rows are virtually stored. Has a sorted value and a pointer to the row (in cluster memory).
                 can have multiple.
  both can be non-unique (except for PK)
  https://www.sqlshack.com/what-is-the-difference-between-clustered-and-non-clustered-indexes-in-sql-server/
  */
  CREATE TABLE Custom.student
(
    id INT IDENTITY(1,1) not null, -- can add primary key here too
    name VARCHAR(50) NOT NULL,
    gender VARCHAR(50) NOT NULL,
    -- DOB datetime NOT NULL,
    total_score INT NOT NULL,
    constraint PK_id primary key clustered (id)
 );

 alter table custom.student
 add constraint name_unique unique(name);

 alter table custom.student
 add constraint genderValue check(gender in ('male','female'));

 alter table custom.student
 add constraint total_score_valid check(total_score >= 0 and total_score <=100);

 alter table custom.student
 add constraint total_score_default default 0 for total_score;

 -- N'...' => literal string w/ reserved chars
 EXECUTE sp_helpindex N'Custom.student';
 
 insert into CustomDatabase.Custom.student
 values ('nick','male', 100);

 insert into CustomDatabase.Custom.student
 values
 ('casey', 'female', 98),
 ('matthew','male', 93),
 ('austin', 'male', 75);

 insert into CustomDatabase.Custom.student
 ([name], gender)
 values ('bill', 'male');
 
 -- no dual table : )
 select 1 as 'number';

 select * from custom.student;

 /*
 CREATE UNIQUE INDEX index1 ON schema1.table1 (column1 DESC, column2 ASC, column3 DESC);
 creates a 3-part non-clustered index where the three parts combined are unique

 clustered: create [unique]* clustered index on <tableName> (<columns>[ASC|DESC]...)
 * may not be applicable but clusters can be non-unique so I believe it's ok
 */
 -- creates non-clustered, non-unique index on name col
 -- ASC|DESC not needed
 create index nameIndex on CustomDatabase.Custom.student (name desc);
 -- unique constraint on name already created an index so it's un-needed for two.
 -- drop index
 drop index if exists nameIndex on CustomDatabase.Custom.student;

 -- queries for database interactivity tests below
 select * 
 from Custom.student
 where gender <> 'male';

 select *, len(name) as length 
 from custom.student
 where len(name) > 5 and gender = 'male';

 -- scalar var used in "anonymous" must be executed in one command
 -- start of anonymous-procedure* 
 -- *: not actual anonymous procedure but it acts like one
 declare @tranName varchar(20);
 select tranName = 'test_transaction';

 begin transaction @tranName
 update Custom.student
 set total_score = total_score + 10
 where total_score = 0;

 update Custom.student
 set total_score = total_score - 10
 where total_score = 10;

 print(concat('rows affected: ', @@ROWCOUNT, ' error code: ', @@ERROR));

 if @@ROWCOUNT <> 0 or @@ERROR <> 0
	begin
		print('rolling back');
		rollback transaction @tranName; -- if no name is given then drop the name part
	end
else
	begin
		print('commiting');
		commit transaction @tranName;
	end
go

delete from Custom.student
where name like 'testBatch%';

insert Custom.student
(name, gender) values ('testBatch1', 'male');

insert Custom.student
(name, gender) values ('testBatch2', 'female');