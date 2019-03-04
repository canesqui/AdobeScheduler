--Migration Script



USE AdobeScheduler;  
--begin tran
GO  
EXEC sp_rename 'Appointments.repititionId', 'repetitionId', 'COLUMN';  
GO  

GO  
EXEC sp_rename 'Appointments.repititionType', 'repetitionType', 'COLUMN';  
GO  

GO  
ALTER TABLE Appointments DROP COLUMN isRep;
GO
--rollback tran

--begin tran 
update a 
	set 
	a.[start] = DATEADD(hour, 5, a.[start]), 
	a.[end] = DATEADD(hour, 5, a.[end]) 
from dbo.Appointments a
--rollback tran 