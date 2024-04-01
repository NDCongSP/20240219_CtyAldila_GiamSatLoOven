SELECT *
  FROM [Oven].[dbo].[FT01]

SELECT *
  FROM [Oven].[dbo].[FT02]

SELECT *
FROM [Oven].[dbo].[FT03]
ORDER BY CreatedDate DESC

SELECT *
  FROM [Oven].[dbo].[FT04]

SELECT *
  FROM [Oven].[dbo].[FT05]

  --truncate table ft01

  --update FT01 set C001=N'[{"Id":1,"Name":"Oven 1","Profiles":[{"Id":1,"Name":"Profile 1","Steps":[{"Id":1,"StepType":1,"Hours":1,"Minutes":30,"Seconds":1,"SetPoint":170.0},{"Id":2,"StepType":3,"Hours":0,"Minutes":50,"Seconds":10,"SetPoint":171.0},{"Id":3,"StepType":5,"Hours":0,"Minutes":0,"Seconds":0,"SetPoint":0.0}]}],"Path":"Local Station/Channel1/Oven1"},{"Id":2,"Name":"Oven 2","Profiles":[{"Id":1,"Name":"Profile 2","Steps":[{"Id":1,"StepType":1,"Hours":1,"Minutes":30,"Seconds":1,"SetPoint":170.0},{"Id":2,"StepType":3,"Hours":0,"Minutes":50,"Seconds":10,"SetPoint":171.0},{"Id":3,"StepType":5,"Hours":0,"Minutes":0,"Seconds":0,"SetPoint":0.0}]}],"Path":"Local Station/Channel1/Oven2"},{"Id":3,"Name":"Oven 3","Profiles":[{"Id":1,"Name":"Profile 3","Steps":[{"Id":1,"StepType":1,"Hours":1,"Minutes":30,"Seconds":1,"SetPoint":170.0},{"Id":2,"StepType":3,"Hours":0,"Minutes":50,"Seconds":10,"SetPoint":171.0},{"Id":3,"StepType":5,"Hours":0,"Minutes":0,"Seconds":0,"SetPoint":0.0}]}],"Path":"Local Station/Channel1/Oven3"},{"Id":4,"Name":"Oven 4","Profiles":[{"Id":1,"Name":"Profile 4","Steps":[{"Id":1,"StepType":1,"Hours":1,"Minutes":30,"Seconds":1,"SetPoint":170.0},{"Id":2,"StepType":3,"Hours":0,"Minutes":50,"Seconds":10,"SetPoint":171.0},{"Id":3,"StepType":5,"Hours":0,"Minutes":0,"Seconds":0,"SetPoint":0.0}]}],"Path":"Local Station/Channel1/Oven4"},{"Id":5,"Name":"Oven 5","Profiles":[{"Id":1,"Name":"Profile 5","Steps":[{"Id":1,"StepType":1,"Hours":1,"Minutes":30,"Seconds":1,"SetPoint":170.0},{"Id":2,"StepType":3,"Hours":0,"Minutes":50,"Seconds":10,"SetPoint":171.0},{"Id":3,"StepType":5,"Hours":0,"Minutes":0,"Seconds":0,"SetPoint":0.0}]}],"Path":"Local Station/Channel1/Oven5"},{"Id":6,"Name":"Oven 6","Profiles":[{"Id":1,"Name":"Profile 6","Steps":[{"Id":1,"StepType":1,"Hours":1,"Minutes":30,"Seconds":1,"SetPoint":170.0},{"Id":2,"StepType":3,"Hours":0,"Minutes":50,"Seconds":10,"SetPoint":171.0},{"Id":3,"StepType":5,"Hours":0,"Minutes":0,"Seconds":0,"SetPoint":0.0}]}],"Path":"Local Station/Channel2/Oven6"},{"Id":7,"Name":"Oven 7","Profiles":[{"Id":1,"Name":"Profile 7","Steps":[{"Id":1,"StepType":1,"Hours":1,"Minutes":30,"Seconds":1,"SetPoint":170.0},{"Id":2,"StepType":3,"Hours":0,"Minutes":50,"Seconds":10,"SetPoint":171.0},{"Id":3,"StepType":5,"Hours":0,"Minutes":0,"Seconds":0,"SetPoint":0.0}]}],"Path":"Local Station/Channel2/Oven7"},{"Id":8,"Name":"Oven 8","Profiles":[{"Id":1,"Name":"Profile 8","Steps":[{"Id":1,"StepType":1,"Hours":1,"Minutes":30,"Seconds":1,"SetPoint":170.0},{"Id":2,"StepType":3,"Hours":0,"Minutes":50,"Seconds":10,"SetPoint":171.0},{"Id":3,"StepType":5,"Hours":0,"Minutes":0,"Seconds":0,"SetPoint":0.0}]}],"Path":"Local Station/Channel2/Oven8"},{"Id":9,"Name":"Oven 9","Profiles":[{"Id":1,"Name":"Profile 9","Steps":[{"Id":1,"StepType":1,"Hours":1,"Minutes":30,"Seconds":1,"SetPoint":170.0},{"Id":2,"StepType":3,"Hours":0,"Minutes":50,"Seconds":10,"SetPoint":171.0},{"Id":3,"StepType":5,"Hours":0,"Minutes":0,"Seconds":0,"SetPoint":0.0}]}],"Path":"Local Station/Channel2/Oven9"},{"Id":10,"Name":"Oven 10","Profiles":[{"Id":1,"Name":"Profile 10","Steps":[{"Id":1,"StepType":1,"Hours":1,"Minutes":30,"Seconds":1,"SetPoint":170.0},{"Id":2,"StepType":3,"Hours":0,"Minutes":50,"Seconds":10,"SetPoint":171.0},{"Id":3,"StepType":5,"Hours":0,"Minutes":0,"Seconds":0,"SetPoint":0.0}]}],"Path":"Local Station/Channel2/Oven10"},{"Id":11,"Name":"Oven 11","Profiles":[{"Id":1,"Name":"Profile 11","Steps":[{"Id":1,"StepType":1,"Hours":1,"Minutes":30,"Seconds":1,"SetPoint":170.0},{"Id":2,"StepType":3,"Hours":0,"Minutes":50,"Seconds":10,"SetPoint":171.0},{"Id":3,"StepType":5,"Hours":0,"Minutes":0,"Seconds":0,"SetPoint":0.0}]}],"Path":"Local Station/Channel3/Oven11"},{"Id":12,"Name":"Oven 12","Profiles":[{"Id":1,"Name":"Profile 12","Steps":[{"Id":1,"StepType":1,"Hours":1,"Minutes":30,"Seconds":1,"SetPoint":170.0},{"Id":2,"StepType":3,"Hours":0,"Minutes":50,"Seconds":10,"SetPoint":171.0},{"Id":3,"StepType":5,"Hours":0,"Minutes":0,"Seconds":0,"SetPoint":0.0}]}],"Path":"Local Station/Channel3/Oven12"},{"Id":13,"Name":"Oven 13","Profiles":[{"Id":1,"Name":"Profile 13","Steps":[{"Id":1,"StepType":1,"Hours":1,"Minutes":30,"Seconds":1,"SetPoint":170.0},{"Id":2,"StepType":3,"Hours":0,"Minutes":50,"Seconds":10,"SetPoint":171.0},{"Id":3,"StepType":5,"Hours":0,"Minutes":0,"Seconds":0,"SetPoint":0.0}]}],"Path":"Local Station/Channel3/Oven13"}]' where Id='0DA5CB40-F84D-4BD3-F908-08DC4C001D57'

  --update ft02 set c000=N'[{"OvenId":1,"OvenName":"Oven 1","Path":"Local Station/Channel1/Oven1","Status":0,"ConnectionStatus":1,"Alarm":0,"Temperature":27948.0,"DoorStatus":1,"ProfileNumber_CurrentStatus":1,"ProfileName":"2121212","ProfileStepNumber_CurrentStatus":1,"ProfileStepType_CurrentStatus":1,"HoursRemaining_CurrentStatus":0,"MinutesRemaining_CurrentStatus":0,"SecondsRemaining_CurrentStatus":0,"TemperatureHighLevel":0.0,"ZIndex":"00000000-0000-0000-0000-000000000000"},{"OvenId":2,"OvenName":"Oven 2","Path":"Local Station/Channel1/Oven2","Status":0,"ConnectionStatus":1,"Alarm":0,"Temperature":27948.0,"DoorStatus":1,"ProfileNumber_CurrentStatus":0,"ProfileName":null,"ProfileStepNumber_CurrentStatus":0,"ProfileStepType_CurrentStatus":5,"HoursRemaining_CurrentStatus":0,"MinutesRemaining_CurrentStatus":0,"SecondsRemaining_CurrentStatus":0,"TemperatureHighLevel":0.0,"ZIndex":"00000000-0000-0000-0000-000000000000"},{"OvenId":3,"OvenName":"Oven 3","Path":"Local Station/Channel1/Oven3","Status":0,"ConnectionStatus":1,"Alarm":0,"Temperature":27949.0,"DoorStatus":1,"ProfileNumber_CurrentStatus":0,"ProfileName":null,"ProfileStepNumber_CurrentStatus":0,"ProfileStepType_CurrentStatus":5,"HoursRemaining_CurrentStatus":0,"MinutesRemaining_CurrentStatus":0,"SecondsRemaining_CurrentStatus":0,"TemperatureHighLevel":0.0,"ZIndex":"00000000-0000-0000-0000-000000000000"},{"OvenId":4,"OvenName":"Oven 4","Path":"Local Station/Channel1/Oven4","Status":0,"ConnectionStatus":1,"Alarm":0,"Temperature":27949.0,"DoorStatus":1,"ProfileNumber_CurrentStatus":0,"ProfileName":null,"ProfileStepNumber_CurrentStatus":0,"ProfileStepType_CurrentStatus":5,"HoursRemaining_CurrentStatus":0,"MinutesRemaining_CurrentStatus":0,"SecondsRemaining_CurrentStatus":0,"TemperatureHighLevel":0.0,"ZIndex":"00000000-0000-0000-0000-000000000000"},{"OvenId":5,"OvenName":"Oven 5","Path":"Local Station/Channel1/Oven5","Status":0,"ConnectionStatus":1,"Alarm":0,"Temperature":27950.0,"DoorStatus":1,"ProfileNumber_CurrentStatus":0,"ProfileName":null,"ProfileStepNumber_CurrentStatus":0,"ProfileStepType_CurrentStatus":5,"HoursRemaining_CurrentStatus":0,"MinutesRemaining_CurrentStatus":0,"SecondsRemaining_CurrentStatus":0,"TemperatureHighLevel":0.0,"ZIndex":"00000000-0000-0000-0000-000000000000"},{"OvenId":6,"OvenName":"Oven 6","Path":"Local Station/Channel2/Oven6","Status":0,"ConnectionStatus":1,"Alarm":0,"Temperature":110.0,"DoorStatus":0,"ProfileNumber_CurrentStatus":0,"ProfileName":null,"ProfileStepNumber_CurrentStatus":0,"ProfileStepType_CurrentStatus":5,"HoursRemaining_CurrentStatus":0,"MinutesRemaining_CurrentStatus":0,"SecondsRemaining_CurrentStatus":0,"TemperatureHighLevel":0.0,"ZIndex":"00000000-0000-0000-0000-000000000000"},{"OvenId":7,"OvenName":"Oven 7","Path":"Local Station/Channel2/Oven7","Status":0,"ConnectionStatus":1,"Alarm":0,"Temperature":110.0,"DoorStatus":0,"ProfileNumber_CurrentStatus":0,"ProfileName":null,"ProfileStepNumber_CurrentStatus":0,"ProfileStepType_CurrentStatus":5,"HoursRemaining_CurrentStatus":0,"MinutesRemaining_CurrentStatus":0,"SecondsRemaining_CurrentStatus":0,"TemperatureHighLevel":0.0,"ZIndex":"00000000-0000-0000-0000-000000000000"},{"OvenId":8,"OvenName":"Oven 8","Path":"Local Station/Channel2/Oven8","Status":0,"ConnectionStatus":1,"Alarm":0,"Temperature":110.0,"DoorStatus":0,"ProfileNumber_CurrentStatus":0,"ProfileName":null,"ProfileStepNumber_CurrentStatus":0,"ProfileStepType_CurrentStatus":5,"HoursRemaining_CurrentStatus":0,"MinutesRemaining_CurrentStatus":0,"SecondsRemaining_CurrentStatus":0,"TemperatureHighLevel":0.0,"ZIndex":"00000000-0000-0000-0000-000000000000"},{"OvenId":9,"OvenName":"Oven 9","Path":"Local Station/Channel2/Oven9","Status":0,"ConnectionStatus":1,"Alarm":0,"Temperature":110.0,"DoorStatus":0,"ProfileNumber_CurrentStatus":0,"ProfileName":null,"ProfileStepNumber_CurrentStatus":0,"ProfileStepType_CurrentStatus":5,"HoursRemaining_CurrentStatus":0,"MinutesRemaining_CurrentStatus":0,"SecondsRemaining_CurrentStatus":0,"TemperatureHighLevel":0.0,"ZIndex":"00000000-0000-0000-0000-000000000000"},{"OvenId":10,"OvenName":"Oven 10","Path":"Local Station/Channel2/Oven10","Status":0,"ConnectionStatus":1,"Alarm":0,"Temperature":110.0,"DoorStatus":0,"ProfileNumber_CurrentStatus":0,"ProfileName":null,"ProfileStepNumber_CurrentStatus":0,"ProfileStepType_CurrentStatus":5,"HoursRemaining_CurrentStatus":0,"MinutesRemaining_CurrentStatus":0,"SecondsRemaining_CurrentStatus":0,"TemperatureHighLevel":0.0,"ZIndex":"00000000-0000-0000-0000-000000000000"},{"OvenId":11,"OvenName":"Oven 11","Path":"Local Station/Channel3/Oven11","Status":0,"ConnectionStatus":1,"Alarm":0,"Temperature":120.0,"DoorStatus":0,"ProfileNumber_CurrentStatus":0,"ProfileName":null,"ProfileStepNumber_CurrentStatus":0,"ProfileStepType_CurrentStatus":5,"HoursRemaining_CurrentStatus":0,"MinutesRemaining_CurrentStatus":0,"SecondsRemaining_CurrentStatus":0,"TemperatureHighLevel":0.0,"ZIndex":"00000000-0000-0000-0000-000000000000"},{"OvenId":12,"OvenName":"Oven 12","Path":"Local Station/Channel3/Oven12","Status":0,"ConnectionStatus":1,"Alarm":0,"Temperature":120.0,"DoorStatus":0,"ProfileNumber_CurrentStatus":0,"ProfileName":null,"ProfileStepNumber_CurrentStatus":0,"ProfileStepType_CurrentStatus":5,"HoursRemaining_CurrentStatus":0,"MinutesRemaining_CurrentStatus":0,"SecondsRemaining_CurrentStatus":0,"TemperatureHighLevel":0.0,"ZIndex":"00000000-0000-0000-0000-000000000000"},{"OvenId":13,"OvenName":"Oven 13","Path":"Local Station/Channel3/Oven13","Status":0,"ConnectionStatus":1,"Alarm":0,"Temperature":120.0,"DoorStatus":0,"ProfileNumber_CurrentStatus":0,"ProfileName":null,"ProfileStepNumber_CurrentStatus":0,"ProfileStepType_CurrentStatus":5,"HoursRemaining_CurrentStatus":0,"MinutesRemaining_CurrentStatus":0,"SecondsRemaining_CurrentStatus":0,"TemperatureHighLevel":0.0,"ZIndex":"00000000-0000-0000-0000-000000000000"}]' where id ='2BB5D6F2-0BB4-438A-BB7B-2AD5501F4FC4'