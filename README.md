# Kohi - Ứng dụng Point-of-sale kinh doanh đồ uống (Lập trình Windows - 22KTPM1)

## Thông tin nhóm
- Nguyễn Minh Hoàng - 22127128
- Trần Nguyễn Phúc Khang - 22127182
- Trần Ngọc Uyển Nhi - 22127313

## Môi trường lập trình
- **Visual Studio 2022 (Community)** 
- **.NET 9.0 SDK**
- **WinUI 3**
- **Đóng gói bằng MSIX**

## Cài Đặt và Chạy Chương Trình
1. **Mở dự án trong Visual Studio**
   - Mở Visual Studio.
   - Chọn **Open a project or solution** và điều hướng đến thư mục dự án.
   - Mở file `Kohi.sln`.

2. **Cấu hình và chạy chương trình**
   - Chọn mode chạy **Debug**.
   - Chạy chương trình dưới dạng **Package**:
      - Chọn Package làm chế độ chạy.
      - Chạy ứng dụng bằng cách nhấn **F5** hoặc chọn **Start Debugging**.

**Lưu ý**: Chương trình phải được chạy dưới dạng Package (không chạy Unpackage) thì mới có thể hoạt động được 
(Tham khảo: https://stackoverflow.com/questions/78379958/operation-is-not-valid-due-to-current-state-of-object-when-calling-windows-stora)

## Đăng Nhập Vào Hệ Thống
Để đăng nhập vào hệ thống, hãy sử dụng tài khoản dưới đây:
- **Username:** `admin`
- **Password:** `admin`

## Chức Năng Chính
#### Xem Danh Sách Sản Phẩm
- Truy cập trang **"Danh mục sản phẩm"** trong mục **"Dữ liệu cơ sở"**.

#### Quản Lý Sản Phẩm
- **Thêm sản phẩm:** Nhấn nút **"Thêm"** để thêm sản phẩm mới.
- **Xóa sản phẩm:** Chọn một sản phẩm và nhấn **"Xóa"** để xóa sản phẩm đó.
- **Sửa sản phẩm:** Chọn một sản phẩm và nhấn **"Sửa"** để chỉnh sửa thông tin sản phẩm.