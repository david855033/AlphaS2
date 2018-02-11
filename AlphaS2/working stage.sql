use alphas2;
--select * from fetch_log
declare @id varchar(10);
set @id='1101';
set @id='=0050';
--select distinct id from level1 ;
--select * from level1 where id = @id;
--select * from level2 where id = @id;
--select * from level3 where id = @id;
--select * from level4 where id = @id;
--select * from level5 order by date , future_price_80;
select * from level6 order by date , future_rank_80;
select * from scoreRef