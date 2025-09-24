# Paradise-be

.net MVC project **Paradise**.

## RUN PROJECT

### 1. Run Docker

Chạy MySQL thông qua Docker Compose:

```bash
docker compose up
```

> Docker sẽ tự động tạo database, user và password theo cấu hình trong file `docker-compose.yml`.

### 2. Connect Workbench

Kết nối MySQL Workbench với database Docker:

* **Host:** `127.0.0.1` (hoặc `localhost`)
* **Port:** theo cấu hình Docker
* **User:** `mvcuser`
* **Password:** `mvcpass`
* **Database:** `mvcdb`

> Cấu hình tương tự như trong file `docker-compose.yml`.

### 3. Connect DBeaver

Kết nối DBeaver với MySQL:

* Sử dụng cùng cấu hình như Workbench.
* Đảm bảo port, user, password và database trùng với Docker.
* Lưu ý Driver properties: PublicKey = true và SSL = false.

### 4. Run App

Chạy ứng dụng:

* Mở IDE (Visual Studio).
* Chạy project như bình thường (`F5` hoặc `dotnet run` nếu dùng .NET).

> Hãy đảm bảo database đã sẵn sàng trước khi chạy ứng dụng.

### Notes

* Nếu thay đổi dữ liệu database, các máy khác phải chạy lại script SQL hoặc migration.
* EF Core First Code có thể được sử dụng để đồng bộ schema nếu cần.
