# Hướng dẫn thay đổi tài khoản Git

## Thông tin hiện tại
- **Username:** khuongdp
- **Email:** khuongdp1402@gmail.com

## Cách thay đổi tài khoản Git

### 1. Thay đổi cho repository hiện tại (Local - chỉ áp dụng cho project này)

#### Thay đổi Username:
```powershell
cd "e:\Project\GiamSatLoOven"
git config user.name "TênMới"
```

#### Thay đổi Email:
```powershell
cd "e:\Project\GiamSatLoOven"
git config user.email "emailmoi@example.com"
```

#### Thay đổi cả hai cùng lúc:
```powershell
cd "e:\Project\GiamSatLoOven"
git config user.name "TênMới"
git config user.email "emailmoi@example.com"
```

### 2. Thay đổi cho toàn bộ hệ thống (Global - áp dụng cho tất cả repositories)

#### Thay đổi Username:
```powershell
git config --global user.name "TênMới"
```

#### Thay đổi Email:
```powershell
git config --global user.email "emailmoi@example.com"
```

#### Thay đổi cả hai cùng lúc:
```powershell
git config --global user.name "TênMới"
git config --global user.email "emailmoi@example.com"
```

### 3. Kiểm tra cấu hình đã thay đổi

#### Kiểm tra cấu hình Local (chỉ repository này):
```powershell
cd "e:\Project\GiamSatLoOven"
git config user.name
git config user.email
```

#### Kiểm tra cấu hình Global:
```powershell
git config --global user.name
git config --global user.email
```

#### Xem tất cả cấu hình:
```powershell
git config --list
```

### 4. Xóa cấu hình (nếu cần)

#### Xóa cấu hình Local:
```powershell
cd "e:\Project\GiamSatLoOven"
git config --unset user.name
git config --unset user.email
```

#### Xóa cấu hình Global:
```powershell
git config --global --unset user.name
git config --global --unset user.email
```

## Lưu ý quan trọng

### 1. Thứ tự ưu tiên cấu hình Git:
- **Local** (repository) > **Global** (hệ thống) > **System** (toàn máy)
- Nếu có cấu hình Local, nó sẽ được ưu tiên sử dụng

### 2. Thay đổi cấu hình không ảnh hưởng đến commits đã tạo:
- Các commits đã tạo trước đó vẫn giữ nguyên thông tin tác giả
- Chỉ các commits mới sẽ sử dụng thông tin tác giả mới

### 3. Nếu cần thay đổi thông tin tác giả cho commits cũ:
```powershell
# Cảnh báo: Chỉ làm khi thực sự cần thiết và đã backup!
git filter-branch --env-filter '
OLD_EMAIL="emailcu@example.com"
CORRECT_NAME="TênMới"
CORRECT_EMAIL="emailmoi@example.com"
if [ "$GIT_COMMITTER_EMAIL" = "$OLD_EMAIL" ]
then
    export GIT_COMMITTER_NAME="$CORRECT_NAME"
    export GIT_COMMITTER_EMAIL="$CORRECT_EMAIL"
fi
if [ "$GIT_AUTHOR_EMAIL" = "$OLD_EMAIL" ]
then
    export GIT_AUTHOR_NAME="$CORRECT_NAME"
    export GIT_AUTHOR_EMAIL="$CORRECT_EMAIL"
fi
' --tag-name-filter cat -- --branches --tags
```

## Ví dụ thực tế

### Ví dụ 1: Thay đổi sang tài khoản mới cho project này
```powershell
cd "e:\Project\GiamSatLoOven"
git config user.name "NguyenVanA"
git config user.email "nguyenvana@example.com"
```

### Ví dụ 2: Thay đổi cho toàn bộ hệ thống
```powershell
git config --global user.name "NguyenVanA"
git config --global user.email "nguyenvana@example.com"
```

### Ví dụ 3: Kiểm tra cấu hình hiện tại
```powershell
cd "e:\Project\GiamSatLoOven"
Write-Host "Local config:"
git config user.name
git config user.email
Write-Host "`nGlobal config:"
git config --global user.name
git config --global user.email
```

## Xác thực với GitHub/GitLab (nếu cần)

### Nếu sử dụng HTTPS:
- Git sẽ yêu cầu nhập username/password hoặc Personal Access Token
- Có thể lưu credentials:
```powershell
git config --global credential.helper wincred
```

### Nếu sử dụng SSH:
- Cần cấu hình SSH key mới:
```powershell
# Tạo SSH key mới
ssh-keygen -t ed25519 -C "emailmoi@example.com"

# Copy public key để thêm vào GitHub/GitLab
cat ~/.ssh/id_ed25519.pub
```

## Troubleshooting

### Lỗi: "fatal: could not read Username"
- **Nguyên nhân:** Git không biết dùng tài khoản nào để push
- **Giải pháp:** 
  - Sử dụng Personal Access Token thay vì password
  - Hoặc cấu hình SSH key

### Lỗi: "Permission denied"
- **Nguyên nhân:** Tài khoản không có quyền truy cập repository
- **Giải pháp:** 
  - Kiểm tra quyền truy cập trên GitHub/GitLab
  - Đảm bảo SSH key hoặc token đã được thêm vào tài khoản

## Tài liệu tham khảo
- [Git Config Documentation](https://git-scm.com/docs/git-config)
- [GitHub: Setting your commit email address](https://docs.github.com/en/account-and-profile/setting-up-and-managing-your-github-user-account/managing-email-preferences/setting-your-commit-email-address)
