-- Cursors
-- (Used to fetch one row at a time)

SELECT * FROM authors;

-- Let's use cursor to backup a table (authors in this case)
CREATE TABLE authors_backup(
	au_id nvarchar(30) primary key,
	au_fname nvarchar(30),
	au_lname nvarchar(30),
	phone nvarchar(20),
	address nvarchar(100),
	city nvarchar(15),
	state nvarchar(10),
	zip int,
	contract int
);

-- Let's use procedure for this
CREATE PROCEDURE BackupAuthors
AS
BEGIN
	declare @au_id varchar(20), @zip varchar(10), @contract bit;
	declare @au_fname varchar(30), @au_lname varchar(30), @phone varchar(20);
	declare @city varchar(20), @state varchar(5), @address varchar(100);
	declare @done int=0;
	declare @cursor cursor;

	set @cursor = cursor for
	select * from authors;

	declare @status int;

	delete from authors_backup;

	open @cursor;

	while @done=0
	begin
		fetch next from @cursor into @au_id, @au_fname, @au_lname, @phone, @address, @city, @state, @zip, @contract;
		set @status = @@FETCH_STATUS;
		if @status != 0 -- Only if status is 0 (next row is fetched successfully)
			set @done=1;
		else
			insert into authors_backup values(@au_id, @au_fname, @au_lname, @phone, @address, @city, @state, @zip, @contract);
	end;
	close @cursor;
	deallocate @cursor;
end;

execute BackupAuthors;

select * from authors_backup;