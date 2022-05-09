use CustomDatabase;
go

create table custom.dog(
	id uniqueidentifier not null default(newsequentialid()) rowguidcol,
	name nvarchar(max) not null,
	breed nvarchar(max) not null,
	gender nvarchar(max) not null,
	constraint genderCheck check (gender in ('male', 'female')),
	constraint dogPK primary key clustered (id)
);

insert into custom.dog
(name, breed, gender)
values
('koko', 'choclate lab', 'female'),
('rylee', 'black lab', 'female'),
('juno', 'terrier', 'male'),
('hudson', 'huskie', 'male');

select * from custom.dog;

go 

create or alter procedure custom.GetAllDogs
as
begin
	select * from custom.dog;
end;
-- exec custom.GetAllDogs;
go

create or alter procedure custom.GetDogById
@id uniqueidentifier
as
begin
	select * from custom.dog
	where @id = id;
end;

exec custom.GetDogById @id = 'C8256731-E1CE-EC11-A133-3C9C0F4BE807';

go

create or alter procedure custom.InsertDog
@id uniqueidentifier = null output, -- 'output' means both input AND output
@name nvarchar(max),
@breed nvarchar(max),
@gender nvarchar(max)
as
begin
	if @id is null
	begin
		set @id = newid();
	end;

	begin try
	if @gender <> 'male' and @gender <> 'female'
		begin
			raiserror('gender is not male or female',16,1);
		end;
	else
		begin
			insert into custom.dog values (@id,@name,@breed,@gender);
		end;
	end try
	begin catch
		print(concat('oops: ' + char(13), error_message()));
		throw; -- throw can only be done inside catch
	end catch
end;

exec custom.InsertDog @name = 'SomeDog', @breed = 'mixed', @gender = 'male';
select * from custom.dog;
delete from custom.dog where name = 'SomeDog';
go

--select substring('nick',0,len('nick') + 1);

create or alter procedure custom.UpdateDogByName
@name nvarchar(max)
as
begin
	update custom.dog set name = concat(name, '!') where name = @name;
	update custom.dog set name = substring(name,0, len(name)) where name like '%!'; -- removes last char [inclusive, exclusive)
end;

exec custom.UpdateDogByName @name = 'koko';
select * from custom.dog;