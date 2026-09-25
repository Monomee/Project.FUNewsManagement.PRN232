# FUNewsManagementSystem - Assignment 01
**Môn học**: PRN232 - Advanced Cross-Platform Web Development with .NET  
**Sinh viên thực hiện**: HoangNV  
**Mã lớp**: SE1930  
**Mã bài thi**: A01 (Assignment 01)  

---

## 1. Giới Thiệu Dự Án

**FUNewsManagementSystem** là hệ thống tin tức và truyền thông nội bộ trường đại học được xây dựng theo kiến trúc phân tán hiện đại, tuân thủ nghiêm ngặt **Kiến trúc 3 lớp (3-Layer Architecture)**:
- **Backend API (`HoangNV_SE1930_A01_BE`)**: ASP.NET Core Web API 8.0, Entity Framework Core 8, Microsoft.AspNetCore.OData 9.5.0, JWT Bearer Authentication, ClosedXML Excel Export Engine, SQL Server.
  - Sử dụng mẫu thiết kế **Repository Pattern** và **Thread-Safe Singleton DAO** (mỗi method tự khởi tạo và giải phóng `FunewsManagementContext` độc lập, triệt tiêu 100% nguy cơ lỗi concurrency khi nhiều request đồng thời).
- **Frontend Web Application (`HoangNV_SE1930_A01_FE`)**: ASP.NET Core Razor Pages 8.0, Bootstrap 5, AJAX/Fetch API đính kèm JWT Bearer Token tự động, Cookie Authentication & Session State.
- **Cơ sở dữ liệu**: SQL Server (`FUNewsManagement`), hỗ trợ đầy đủ khóa chính, khóa ngoại, quan hệ n-n junction table `NewsTag`, danh mục phân cấp cha - con.

---

## 2. Hướng Dẫn Cấu Hình & Cài Đặt

### 2.1. Yêu Cầu Môi Trường
- .NET SDK 8.0 trở lên.
- Microsoft Visual Studio 2022 (hoặc VS Code với C# Dev Kit).
- Microsoft SQL Server 2016 trở lên (LocalDB, SQLEXPRESS, hoặc Standard).

### 2.2. Khởi Tạo Cơ Sở Dữ Liệu
Tập lệnh SQL đầy đủ lược đồ và dữ liệu mẫu mang ý nghĩa thực tế đã được chuẩn bị tại file `FUNewsManagement_Schema_And_Data.sql`.

Chạy lệnh sau trên PowerShell:
```powershell
Invoke-Sqlcmd -ServerInstance 'MONOME\SQLEXPRESS' -InputFile 'FUNewsManagement_Schema_And_Data.sql'
```
*(Hoặc mở SQL Server Management Studio (SSMS), kết nối tới SQL Server của bạn và thực thi file `FUNewsManagement_Schema_And_Data.sql`).*

Chuỗi kết nối mặc định trong `HoangNV_SE1930_A01_BE/HoangNV_SE1930_A01_BE/appsettings.json`:
```json
"ConnectionStrings": {
  "MyCnn": "Data Source=MONOME\\SQLEXPRESS;Initial Catalog=FUNewsManagement;Trusted_Connection=SSPI;Encrypt=false;TrustServerCertificate=True"
}
```
*(Nếu sử dụng máy tính khác, chỉ cần thay đổi `Data Source` cho phù hợp).*

---

## 3. Hướng Dẫn Khởi Chạy Cả 2 Solution Cùng Lúc

### Cách 1: Khởi chạy bằng Terminal / PowerShell (Nhanh & Ổn định nhất)
Mở 2 cửa sổ terminal tại thư mục gốc dự án:

**Terminal 1 - Khởi chạy Backend Web API:**
```powershell
cd HoangNV_SE1930_A01_BE\HoangNV_SE1930_A01_BE
dotnet run --launch-profile http
```
- **API Base URL**: `http://localhost:5132`
- **Swagger Documentation**: `http://localhost:5132/swagger`

**Terminal 2 - Khởi chạy Frontend Web App:**
```powershell
cd HoangNV_SE1930_A01_FE\HoangNV_SE1930_A01_FE
dotnet run --launch-profile http
```
- **Web App URL**: `http://localhost:5237`

### Cách 2: Khởi chạy bằng Visual Studio
1. Mở cửa sổ Visual Studio thứ nhất: Chọn file `HoangNV_SE1930_A01_BE/HoangNV_SE1930_A01_BE.sln` -> Nhấn **Ctrl+F5** để chạy API.
2. Mở cửa sổ Visual Studio thứ hai: Chọn file `HoangNV_SE1930_A01_FE/HoangNV_SE1930_A01_FE.sln` -> Nhấn **Ctrl+F5** để chạy Web App.

---

## 4. Danh Sách Tài Khoản Thử Nghiệm Theo Từng Vai Trò (Roles)

| Vai Trò (Role) | Email Đăng Nhập | Mật Khẩu | Quyền Hạn & Trang Điều Hướng Mặc Định |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@FUNewsManagementSystem.org` | `@@abc123@@` | - Đọc từ `appsettings.json`<br>- Toàn quyền quản lý tài khoản (`/Admin/Accounts`)<br>- Xem thống kê KPI, biểu đồ gom nhóm & xuất file Excel (`/Admin/Reports`) |
| **Staff** (`Role = 1`) | `IsabellaDavid@FUNewsManagement.org` | `@1` | - Quản lý bài viết & nhân bản tin tức (`/Staff/Articles`)<br>- Quản lý danh mục & danh mục con (`/Staff/Categories`)<br>- Quản lý thẻ tag (`/Staff/Tags`)<br>- Quản lý thông tin & đổi mật khẩu cá nhân (`/Staff/Profile`) |
| **Lecturer** (`Role = 2`) | `EmmaWilliam@FUNewsManagement.org` | `@1` | - Tra cứu và đọc tin tức (`/Lecturer/Articles`)<br>- Chế độ **Read & Search Only**, ẩn toàn bộ nút Thêm/Sửa/Xóa/Nhân bản |
| **Guest / Public** | *(Không cần đăng nhập)* | *(Trống)* | - Trang chủ tin tức công khai (`/`)<br>- Lọc theo danh mục Active, tìm kiếm từ khóa<br>- Xem chi tiết bài viết & **Top 3 bài viết liên quan** |

*(Các tài khoản thử nghiệm khác trong database: `OliviaJames@FUNewsManagement.org` / `@1` (Lecturer), `MichaelCharlotte@FUNewsManagement.org` / `@1` (Staff), `SteveParis@FUNewsManagement.org` / `@1` (Staff)).*

---

## 5. Tổng Quan API Endpoints & Mẫu Truy Vấn OData

### 5.1. Bảng Tổng Hợp Endpoint API
| Phương thức | Endpoint | Quyền hạn | Mô tả nghiệp vụ |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/login` | Anonymous | Đăng nhập lấy JWT Bearer token và thông tin người dùng |
| `GET` | `/api/auth/me` | Authenticated | Lấy thông tin tài khoản hiện tại |
| `GET` | `/api/news` hoặc `/api/newsarticle` | Anonymous (OData) | Lấy danh sách tin tức (hỗ trợ OData `$filter`, `$orderby`, `$top`, `$skip`) |
| `GET` | `/api/news/{id}` | Anonymous | Xem chi tiết bài viết |
| `GET` | `/api/news/{id}/related` | Anonymous | Thuật toán gợi ý **Top 3 bài viết liên quan** |
| `POST` | `/api/news` | Staff | Tạo bài viết mới (tự động gán CreatedByID, CreatedDate) |
| `PUT` | `/api/news/{id}` | Staff | Cập nhật bài viết (tự động gán UpdatedByID, ModifiedDate) |
| `DELETE` | `/api/news/{id}` | Staff | Xóa bài viết (tự động xóa cascade liên kết trong `NewsTag`) |
| `POST` | `/api/news/{id}/duplicate` | Staff | Nhân bản bài viết thành bản sao `[Copy] ...` |
| `GET` | `/api/category` hoặc `/api/categories` | Anonymous (OData) | Lấy danh sách danh mục (kèm số lượng bài viết) |
| `POST` | `/api/category` | Staff | Tạo danh mục (chặn trùng tên con cùng cha) |
| `PUT` | `/api/category/{id}` | Staff | Sửa danh mục (**chặn đổi ParentCategoryID nếu đã có bài viết**) |
| `DELETE` | `/api/category/{id}` | Staff | Xóa danh mục (**chặn xóa nếu danh mục đang chứa bài viết**) |
| `GET` | `/api/tag` hoặc `/api/tags` | Anonymous (OData) | Lấy danh sách thẻ tag |
| `POST` | `/api/tag` | Staff | Tạo tag mới (chặn trùng `TagName`) |
| `PUT` | `/api/tag/{id}` | Staff | Sửa tag |
| `DELETE` | `/api/tag/{id}` | Staff | Xóa tag (**chặn xóa nếu tag đang gắn với bài viết**) |
| `GET` | `/api/account` hoặc `/api/accounts` | Admin (OData) | Lấy danh sách tài khoản hệ thống |
| `POST` | `/api/account` | Admin | Tạo tài khoản mới (chặn trùng email) |
| `PUT` | `/api/account/{id}` | Admin | Cập nhật tài khoản (chặn trùng email) |
| `DELETE` | `/api/account/{id}` | Admin | Xóa tài khoản (**chặn xóa nếu tài khoản đã tạo bài viết**) |
| `PUT` / `PATCH` | `/api/account/{id}/password` | Admin, Staff | Đổi mật khẩu (**bắt buộc xác thực đúng mật khẩu hiện tại**) |
| `GET` | `/api/report/statistics` | Admin | Báo cáo thống kê số lượng theo ngày, gom nhóm chuyên mục & tác giả |
| `GET` | `/api/report/export-excel` | Admin | Xuất báo cáo thống kê ra file Excel `.xlsx` chuyên nghiệp |

### 5.2. Mẫu Cú Pháp Truy Vấn OData
- Lọc bài viết đang Active:  
  `GET /api/news?$filter=newsStatus eq true`
- Sắp xếp ngày tạo giảm dần và phân trang:  
  `GET /api/news?$orderby=createdDate desc&$skip=0&$top=5`
- Tìm kiếm từ khóa theo tiêu đề hoặc headline:  
  `GET /api/news?$filter=contains(newsTitle, 'Alumni') or contains(headline, 'Alumni')`
- Lọc bài viết theo danh mục cha hoặc con:  
  `GET /api/news?$filter=categoryId eq 1 and newsStatus eq true`

---

## 6. Các Ràng Buộc Nghiệp Vụ & Toàn Vẹn Dữ Liệu Đã Được Kiểm Thử

Hệ thống đã trải qua bộ kiểm thử tự động **32/32 Test Cases đạt chuẩn 100%**:
1. **Xác thực mật khẩu cũ khi đổi Pass (Mục 4.2)**: Nhập sai mật khẩu hiện tại sẽ bị từ chối với mã **HTTP 400 Bad Request** và thông báo *"The current password is not correct."*
2. **Chặn sửa ParentCategoryID khi Category đã có bài viết (Mục 4.3)**: Khi cố gắng đổi danh mục cha của danh mục đang gắn với bài viết, hệ thống chặn lại với **HTTP 400 Bad Request** và thông báo *"Cannot change the parent category because category '...' is already associated with existing news articles."*
3. **Chặn xóa Category vướng bài viết (Mục 4.3)**: Category đang có bài viết không thể bị xóa -> **HTTP 400 Bad Request**.
4. **Chặn xóa Account đã tạo bài viết (Mục 4.2)**: Account đã tạo bài viết không thể bị xóa -> **HTTP 400 Bad Request**.
5. **Chặn xóa Tag gắn với bài viết (Mục 4.5)**: Tag đang được dùng trong `NewsTag` không thể bị xóa -> **HTTP 400 Bad Request**.
6. **Chặn trùng lặp tên (Mục 5.4)**:
   - Chặn trùng `AccountEmail` khi tạo hoặc cập nhật tài khoản.
   - Chặn trùng `TagName` khi tạo thẻ tag.
   - Chặn trùng tên danh mục con trong cùng một danh mục cha (`ParentCategoryID`). Cùng tên nhưng khác cha thì được chấp nhận.
7. **Phân quyền Lecturer (Mục 4.1)**: Chỉ có quyền đọc và tra cứu tin tức, toàn bộ nút tạo/sửa/xóa bị ẩn và API chặn với mã **HTTP 403 Forbidden**.
8. **Thuật toán gợi ý Top 3 liên quan (Mục 4.9)**: Tự động tìm kiếm các bài viết Active cùng chuyên mục hoặc chung tag, trả về tối đa 3 bài viết có độ tương đồng cao nhất.
9. **Xuất báo cáo Excel (Mục 4.7, 5.8)**: Sử dụng **ClosedXML** định dạng tiêu đề, KPI card, bảng gom nhóm chuyên mục, bảng gom nhóm tác giả, và bảng chi tiết bài viết ra file `.xlsx`.

---

## 7. Thư Mục Minh Chứng Screenshots & Sample Outputs (Mục 5.10)

Theo yêu cầu của đề bài tại Mục 5.10, các ảnh chụp giao diện và kết quả mẫu được lưu trữ tại thư mục `screenshots/` bao gồm:
1. `screenshots/01_Public_News_Portal.png`: Giao diện trang chủ tin tức công khai cho Guest.
2. `screenshots/02_Related_News_Modal.png`: Hộp thoại chi tiết bài viết kèm Top 3 bài viết liên quan.
3. `screenshots/03_Admin_Account_Management.png`: Giao diện quản lý tài khoản với Modal Thêm/Sửa/Đổi pass.
4. `screenshots/04_Staff_Article_Management.png`: Giao diện quản lý tin tức, màu trạng thái Active/Inactive và chọn Tags.
5. `screenshots/05_Staff_Category_Management.png`: Giao diện quản lý danh mục cha và danh mục con.
6. `screenshots/06_Lecturer_Read_Only_View.png`: Giao diện xem tin của Giảng viên (ẩn hoàn toàn các nút thao tác).
7. `screenshots/07_Admin_Report_And_Excel.png`: Báo cáo thống kê số liệu và file Excel `.xlsx` được xuất.
8. `screenshots/08_FK_Constraint_Delete_Blocked.png`: Thông báo lỗi Toast khi bị chặn xóa do vướng khóa ngoại.
