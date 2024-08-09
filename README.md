# 20240219_CtyAldila_GiamSatLoOven

SQL Server 14
DB name: Oven
MD5 secret: PTAut0m@t!0n30!)@)20
conStr" qFgsKRXrOBXrpXLXV/PyMGuUw2HscP3Q/TloIHzSy62+hoLwOzXqyds8oOCa+GuAR/RjEhPAc+7VHDfNYB0vgTks32Ax652Ts4Ygi1wvVoBypvDtyRLmcpLk9zddZBDMvOmr3hP4jW8=

Đường dây lan cắm vào switch ở chỗ sát cửa phòng ăn, port 4
PLC: 192.168.160.32 - COM20
CH1(1-->5): 192.168.160.33 - COM21
CH2(6-->10): 192.168.160.34 - COM22
CH3(11-->13): 192.168.160.35 - COM23

DB:
Server=192.168.96.8;Database=oven;User Id=mfg;Password=Mfg@321!;TrustServerCertificate=True;

ất cả các dự án giám sát trang trại của a Thám đều nằm trong này
de host IIS cac ung dung ASP .net core thi mays tinhs can caif cai nay: dotnet-hosting-7.0.10-win
Cai moi truong de publish IIS:
- dotnet-hosting-7.0.14-win.exe
- aspnetcore-runtime-7.0.14-win-x64
- Hosting bundle
- URL Rewrite Module 2.1

Web API port 8082
web UI port 8083


 item.ProfileStepType_CurrentStatus = e.NewValue == "1" ? EnumProfileStepType.RampTime
                                     : e.NewValue == "2" ? EnumProfileStepType.RampRate
                                     : e.NewValue == "3" ? EnumProfileStepType.Soak
                                     : e.NewValue == "4" ? EnumProfileStepType.Jump
                                     : EnumProfileStepType.End;