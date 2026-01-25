# 20240219_CtyAldila_GiamSatLoOven

SQL Server 14
DB name: Oven
MD5 secret: PTAut0m@t!0n30!)@)20
conStr" qFgsKRXrOBXrpXLXV/PyMGuUw2HscP3Q/TloIHzSy62+hoLwOzXqyds8oOCa+GuAR/RjEhPAc+7VHDfNYB0vgTks32Ax652Ts4Ygi1wvVoBypvDtyRLmcpLk9zddZBDMvOmr3hP4jW8=

Kiến trúc hệ thống:
|---Web
|       |---Hệ thống lò Oven
|       |---Hệ thống giám sát thời gian chạy các bước của máy Revo + auto rolling
|
|---Scada app
        |---Ap scada chạy trên server
        |       |---Kết hợp lò oven và giám sát thời gian chạy step cảu máy auto rolling
        |       |---EasyDriver: kết hợp thêm các máy auto rolling (modbus TCP/IP)
        |       |       |---Máy autoroling kết nối với HMI có sẵn của máy, nó đã tính toán hết các thông tin về thời gian chạy của từng bước rồi
        |
        |---App chạy riêng biệt từng máy cho các máy Revo Goft
        |       |---Chỉ chạy riêng cho từng máy, đọc data master từ file access, điều khiển chạy PLC và log thời gian chạy dừng từng bước vào DB
        |       |---EasyDriver chỉ kết nối với 1 PLC của máy đó (modbus TCP/IP).
        |
------------------------------------------------------------------
Hệ thống giám sát lò OVEN
------------------------------------------------------------------
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

PLC: kiểm tra mất kết nối đến Server
- D203 = 1 and D204 = 1: Enanble check
D203 ghi từ server xuống, để on/off check
D204 ghi từ server xuống hoặc nhấn cùng lúc 3 nút trên tủ
Khi check connection hoatj động sẽ on Y017

-----------------------------------------------------------------------
Hệ thống giám sát thời gian chạy máy Revo Goft và auto rolling
-----------------------------------------------------------------------
Revo
    |-Revo_1:
    |   |-IP:192.168.1.220
    |-Revo_2
    |   |-IP:192.168.1.221
    |-Revo_3
    |   |-IP:192.168.1.222
    |-Revo_4
    |   |-IP:192.168.1.223
    |-Revo_5
    |   |-IP:192.168.1.224
    |-Revo_6
    |   |-IP:192.168.1.225

máy này cần lập trình PLC
đọc part word từ file access, sau đó lấy thồn tin về góc quay và tốc độ quay/2 truyền xuống PLC hoạt động. rồi lấy thông tin phản hồi START/STOP STEP từ PLC để log vào DB thời gian chạy dừng của các bước theo PART + WORK
-------------------------------------------------------------------------------------------
Auto rolling
    |-Auto_Rolling_1
    |   |-IP:192.168.1.226
    |-Auto_Rolling_2
    |   |-IP:192.168.1.227
    |-Auto_Rolling_3
    |   |-IP:192.168.1.228

máy auto rolling đac có HMI tính toán các thông số chạy máy hết rồi, nên chỉ cẩn kết nối với HMI để lấy thông tin lên lưu vào DB
