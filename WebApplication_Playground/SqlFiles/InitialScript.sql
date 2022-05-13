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

  /*
  CREATE TABLE Custom.student
(
    id INT IDENTITY(1,1) not null, -- can add primary key here too
    name VARCHAR(50) NOT NULL,
    gender VARCHAR(50) NOT NULL,
    -- DOB datetime NOT NULL,
    total_score INT NOT NULL,
    constraint PK_id primary key clustered (id)
 );*/

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

SELECT CONVERT(varchar, '2017-08-25', 101);
select geography::Point(47.65100, -122.34900, 4326);

/*
create table Custom.GeoPoint(
	id uniqueidentifier not null default(newid()),
	long float null,
	lat  float null,
	geo as convert(geography, case when lat is not null and long is not null then geography::Point([lat],long,4326) end, 0),
	constraint geoPkId primary key clustered (id)
);*/

-- drop table Custom.GeoPoint;
delete from Custom.GeoPoint;
insert into Custom.GeoPoint
(lat,long)
values
(47.65100, -122.34900),
(null,null);

select * from Custom.GeoPoint;

-- rowguidcol explained
-- https://www.codeproject.com/Tips/125526/Using-ROWGUIDCOL-in-SQL-Server
/*
create table custom.Person(
	id uniqueidentifier not null default(newsequentialid()) rowguidcol,  -- uniqueidentifer w/ newid()
	name nvarchar(max) not null
);
*/
-- inserted in different command
insert into [custom].person
(name) values ('nick'),('casey');

-- inserted in different command
insert into Custom.person (name) values ('bob');

-- inserted in different command
insert into [custom].person
(name) values ('billy'),('samwise');

-- all id values are different BUT the ones inserted in sequence are similar
select * from custom.Person;

-- you can query directly off of the rowguidcol (no col ref)
select rowguidcol, name from custom.Person;

update custom.person
set
	name = SUBSTRING(name,0,2);

select * from custom.Person;

delete from custom.Person;
/*
create table custom.dog(
	id uniqueidentifier not null default(newsequentialid()) rowguidcol,
	name nvarchar(max) not null,
	breed nvarchar(max) not null,
	gender nvarchar(max) not null,
	constraint genderCheck check (gender in ('male', 'female')),
	constraint dogPK primary key clustered (id)
);*/
go

insert into custom.dog
(name, breed, gender)
values
('koko', 'choclate lab', 'female'),
('rylee', 'black lab', 'female'),
('juno', 'terrier', 'male'),
('hudson', 'huskie', 'male');

select * from custom.dog;

create table custom.EmployeeRecursive(
	id uniqueidentifier default(newsequentialid()) rowguidcol,
	name nvarchar(max) not null,
	level nvarchar(max) not null check(level in ('owner', 'branch manager', 'entry-level')),
	reportsTo uniqueidentifier null,
	constraint empId primary key clustered (id),
	constraint bossFK foreign key (id) references custom.EmployeeRecursive(id)
		on delete no action -- no action = restrict. others -> cascade, set null, set default
		-- on update cascade -- same
);

alter table custom.EmployeeRecursive
add constraint fkNotSelf check (reportsTo <> id);

insert into custom.EmployeeRecursive
(name, level, reportsTo)
values
('elon', 'owner', null),
('billy', 'branch manager', null),
('bob', 'branch manager', null),
('joe', 'entry-level', null),
('samwise', 'entry-level', null);

update custom.EmployeeRecursive
set
	reportsTo = (select id from custom.EmployeeRecursive where name = 'bob')
where
	name = 'samwise';

select * from custom.EmployeeRecursive;

go

-- data types of columns (including length) MUST match
-- cast strings, or other literals when 'anchor' type error occurs
create or alter function custom.EmployeeRecursiveTest()
returns table
as
	return with cte as (
		select *, cast(' ' as nvarchar(max)) as path from custom.EmployeeRecursive where reportsTo is null
		union all
		select e.*, cast(concat(e.name, '->', cte.name) as nvarchar(max)) as path from custom.EmployeeRecursive e, cte where e.reportsTo = cte.id
		
	) select * from cte;

go
create or alter function custom.AddOne(@num int = 0)
returns int
as
begin
	return @num + 1;
end;

go

print(custom.AddOne(3));
select * from custom.EmployeeRecursiveTest();