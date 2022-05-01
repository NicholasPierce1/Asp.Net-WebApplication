create or alter procedure Custom.GetAllStudents
as
begin
	select * from Custom.student;
end;

exec Custom.GetAllStudents;

drop procedure GetAllStudents;
go;

-- doc on try-catch: https://docs.microsoft.com/en-us/sql/t-sql/language-elements/try-catch-transact-sql?view=sql-server-ver15
create or alter procedure Custom.GetStudentsByGender
@gender varchar(10) = 'male' -- default value
as
begin
	-- declare @name varchar = 'male';
	-- set @name = 'malee';
	begin try
		declare @num float = 1/ 10;

		select *
		from Custom.student
		where gender = @gender;
	end try
	begin catch
		print(
			concat(
				'error num: ', error_number(),
				(char(13) +'severity: '), error_severity(),
				(char(13) +'state: '), error_state(),
				char(13) + 'procedure: ', error_procedure(),
				char(13) + 'line: ', error_line(),
				char(13) + 'message: ', error_message()
			)
		)
	end catch
end;
go;
exec custom.GetStudentsByGender; -- using default
exec custom.GetStudentsByGender 'male';

go

create or alter procedure Custom.UpdateStudentWithLowTestScore
@threshold int,
@toIncrease bit -- = 'TRUE', -- can be default w/ automatic conversion to 'TRUE'/1 or 'FALSE'/0
as
begin
	declare @rowCount int;
	begin transaction
	begin try
		if(@toIncrease = 1) -- does not need begin/ end if 1 line
		begin
			update Custom.student
			set total_score = total_score + 10
			where total_score = @threshold;
			set @rowCount = @@ROWCOUNT;
		end
		else
		begin
			update Custom.student
			set total_score = total_score - 10
			where total_score = @threshold;
			set @rowCount = @@ROWCOUNT;
		end
	
		commit transaction;
		print(concat('in sp: ', @rowCount, ' global: ', @@rowcount));
		return @rowCount;
	end try
	begin catch
		print(concat('error number: ', error_number(), char(13) + 'msg: ' + error_message()));
		rollback transaction;
		return -1;
	end catch
end;
go

declare @returnvaluee int;
set @returnvaluee = 1;
--exec @returnvaluee = custom.UpdateStudentWithLowTestScore 0, 'TRUE';
exec @returnvaluee = custom.UpdateStudentWithLowTestScore 10, 'FALSE';
print @returnvaluee
exec custom.GetAllStudents;
go