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
1. **Docker compose**
    - Mở tại thư mục `Kohi/Docker`
    - Chạy command
    ```sh
    docker compose up
    ```
    Command sẽ tạo hệ cơ sở dữ liệu PostgreSQL và trình quản lý cơ sở dữ liệu GUI Adminer tại [http://localhost:8080](http://localhost:8080).
2. **PostgREST**
    - Chạy PostgREST với file cấu hình nằm trong thư mục `Kohi/Docker/config`
    ```sh
    postgrest ...\Kohi\Docker\config\postgrest.conf
    ```
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

## Chức Năng Chính
Tham khảo báo cáo