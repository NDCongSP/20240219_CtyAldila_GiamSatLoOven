# 20240219_CtyAldila_GiamSatLoOven

SQL Server 14
DB name: Oven
MD5 secret: PTAut0m@t!0n30!)@)20
conStr" qFgsKRXrOBXrpXLXV/PyMGuUw2HscP3Q/TloIHzSy62+hoLwOzXqyds8oOCa+GuAR/RjEhPAc+7VHDfNYB0vgTks32Ax652Ts4Ygi1wvVoBypvDtyRLmcpLk9zddZBDMvOmr3hP4jW8=

-- Index chính cho bộ key truy vấn
CREATE NONCLUSTERED INDEX IX_FT09_ShaftNum_RevoId_StepId
ON FT09 (ShaftNum, RevoId, StepId)
INCLUDE (StartedAt, EndedAt, TotalTime, StepName);

-- Index phụ nếu cần lọc theo thời gian
CREATE NONCLUSTERED INDEX IX_FT09_CreatedAt
ON FT09 (CreatedAt);
-----------------------------------------------------------------------------------------------------------------------
DB OVEN: oven-revo-auto rolling-temperature-sanding
Server=Server=192.168.96.22;Database=oven;User Id=mfg;Password=Mfg@321!;TrustServerCertificate=True;

DB ALD_MFG: lấy thông tin của các máy đo D Frequency
Server=Server=192.168.96.8;Database=ALD_MFG;User Id=mfg;Password=Mfg@321!;TrustServerCertificate=True;

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
        |---App máy Auto rolling chạy riêng 1 máy cho 3 máy auto rolling.
------------------------------------------------------------------
Hệ thống giám sát lò OVEN
------------------------------------------------------------------
Đường dây lan cắm vào switch ở chỗ sát cửa phòng ăn, port 4

CH1(1-->5): 192.168.106.33 - COM24
CH2(6-->10): 192.168.106.34 - COM22
CH3(11-->13): 192.168.106.35 - COM23
CH4 (PLC): 192.168.106.32 - COM20

nwtmask: 255.255.255.0
Gateway: 192.168.106.1


de host IIS cac ung dung ASP .net core thi mays tinhs can caif cai nay: dotnet-hosting-7.0.10-win
Cai moi truong de publish IIS:
- dotnet-hosting-7.0.14-win.exe
- aspnetcore-runtime-7.0.14-win-x64
- Hosting bundle
- URL Rewrite Module 2.1

server host: 192.168.96.22
Web API port 8082
web UI port 8083

ip pc host production: 192.168.106.40
API port 8082
UI port 8083


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
-----------------------------------------------------------------------
Hệ thống giám sát thời gian chạy máy Revo Goft và auto rolling
-----------------------------------------------------------------------
EasyDrriverServer dùng Byte Order là Hight Byte First / Low Word First. bởi vì PLC wecon nó dùng Low Word First.

Revo
    |-Revo_1:
    |   |-IP:192.168.1.220

máy này cần lập trình PLC

đọc part word từ file access, sau đó lấy thồn tin về góc quay và tốc độ quay/2 truyền xuống PLC hoạt động. rồi lấy thông tin phản hồi START/STOP STEP từ PLC để log vào DB thời gian chạy dừng của các bước theo PART + WORK
-------------------------------------------------------------------------------------------
Auto rolling: HMI GOT2000, PLC-Q series MC-Protocol port 8000 để kết nối PC
    |-Auto_Rolling_1: AUTO_ROLLING_3073_6_2_2024
        |-HMI:192.168.11.11
        |-PLC: 192.168.11.1
        |-HMI nhỏ: 192.168.11.21
    |-Auto_Rolling_2: AUTO_ROLLING_3082_6_2_2024
        |-HMI:192.168.11.12
        |-PLC: 192.168.11.2
        |-HMI nhỏ: 192.168.11.22
    |-Auto_Rolling_3: AUTO_ROLLING_3083_6_2_2024
        |-HMI:192.168.11.13
        |-PLC: 192.168.11.3 --MC Protocol
        |-HMI nhỏ: 192.168.11.23

máy auto rolling đac có HMI tính toán các thông số chạy máy hết rồi, nên chỉ cẩn kết nối với HMI để lấy thông tin lên lưu vào DB

command test đọc MC-Protocol chat power cell
try {
    $client = New-Object System.Net.Sockets.TcpClient
    $client.Connect("192.168.11.3", 8000)
    $stream = $client.GetStream()
    $stream.ReadTimeout = 3000

    # 3E Binary frame: Read D162, 1 word
    $frame = [byte[]](
        0x50, 0x00,        # Subheader
        0x00,              # Network No
        0xFF,              # PC No (self-station)
        0xFF, 0x03,        # Request I/O No
        0x00,              # Request station No
        0x0C, 0x00,        # Request data length (12 bytes)
        0x10, 0x00,        # Monitoring timer
        0x01, 0x04,        # Command: Batch Read
        0x00, 0x00,        # Subcommand: word units
        0xA2, 0x00, 0x00,  # D162 address (162 = 0xA2)
        0xA8,              # Device code: D register
        0x01, 0x00         # 1 point
    )
    $stream.Write($frame, 0, $frame.Length)

    $buf = New-Object byte[] 64
    $n = $stream.Read($buf, 0, 64)
    $hex = ($buf[0..($n-1)] | ForEach-Object { "{0:X2}" -f $_ }) -join " "
    Write-Host "Bytes received: $n"
    Write-Host "Response: $hex"
    $client.Close()
} catch {
    Write-Host "Error: $_"
}


-----------------------------------------------------------------------
-----------------------------------------------------------------------
Giám sát nhiệt độ nhà máy
-----------------------------------------------------------------------
CÓ tổng cộng 16 vị trí giám sát, trong đó có 3 kho lạnh
Dung Lora chia làm 2 channel, chuyển đổi 485-> TCP dùng 30nedata NP301 | user: admin - admin
ID modbus của thiết bị bằng chính thứ tự của vị trí đo
Chay chung EasyDriver với lò Oven

 Channel1 - IP: 192.168.106.36 - LORA CH 17 - COM25
    - VT1, VT2, VT3, VT4, VT5, VT6, VT7, VT14
 Channel2 - IP: 192.168.106.37 - LORA CH 14 - COM26
    - VT8, VT9, VT10, VT11, VT12, VT13, VT15, VT16

-----------------------------------------------------------------------
-----------------------------------------------------------------------
AUTO SANDING
-----------------------------------------------------------------------
--Frequency min max
select Freq_LL,Freq_UL, * from part where id = 10933

--TIP OD
select * from PartZM _pz
	left join ZMmeasType _zt on _zt.ID = _pz.ZMID
 where _pz.PartID =10933;


--LEnght
select  * from [PartNewSetting] where PartId = 10933