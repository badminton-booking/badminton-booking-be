# Configurations

Đặt các `IEntityTypeConfiguration<T>` tại đây. `AppDbContext` tự nạp toàn bộ configuration và chuyển tên cột sang `snake_case`.

Hai unique index trên cặp User không phụ thuộc thứ tự dùng biểu thức PostgreSQL `LEAST/GREATEST`, nên được khai báo trong [`../Sql/chat-pair-indexes.sql`](../Sql/chat-pair-indexes.sql). Chạy script này sau khi tạo bảng bằng SQL thủ công. Các configuration và script không tự tạo hoặc thay đổi database; dự án hiện không dùng migration.
