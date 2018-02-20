use alphas2;
--select * from fetch_log order by fetch_datetime desc, date desc
--select * from stock_list
declare @id varchar(10);
declare @date date;
set @id='=0050';
set @date = '2007-1-1'
--set @id='=0050';
--select distinct id from level1 ;
--select * from level1 where id = @id and date >=@date;
--select * from level2 where id = @id and date >=@date;
--select * from level3 where id = @id and date >=@date;
--select * from level4 where id = @id and date >=@date;
--select * from level5 where date >= @date order by date , future_price_80;
--select * from level6 where date >= @date order by date , future_rank_80 ;
--select * from level7;
--select * from scoreRef
--select * from level6
--join level3 on level6.id=level3.id and level6.date=level3.date
--join level4 on level6.id=level4.id and level6.date=level4.date
--join level5 on level6.id=level5.id and level6.date=level5.date
--order by ba_mean_3

select id, count(id), max(date) as max, min(date) as min from level1 group by id order by id