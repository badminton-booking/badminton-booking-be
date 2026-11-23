# KIẾN TRÚC DỰ ÁN — RallyHub (be_booking_badminton)

> Clean Architecture dạng **modular monolith**, 7 project, học theo style `buildx-be`. Light CQRS bằng MediatR + pipeline behaviors, Result pattern, DomainException + MessageCode, EF Core + PostgreSQL.
>
> Quy ước: mỗi tầng là 1 project riêng. Code tổ chức theo **feature slice** (mỗi nghiệp vụ một folder cùng tên ở mọi tầng), không gom theo kỹ thuật.

---

## 1. Bảy tầng và luật phụ thuộc

Mũi tên = "được phép tham chiếu tới". Tầng trong **không biết** tầng ngoài.

```
                 ┌─────────────────────────────┐
                 │            API              │  host: Program, DI, middleware
                 └──────────────┬──────────────┘
          ┌─────────────────────┼─────────────────────┐
          ▼                     ▼                     ▼
   ┌────────────┐      ┌────────────────┐     ┌────────────────┐
   │Presentation│      │ Infrastructure │     │  Persistence   │
   │  (endpoints)│     │ (SePay, mail…) │     │ (EF, DbContext)│
   └──────┬─────┘      └────────┬───────┘     └───────┬────────┘
          │                     │                     │
          ▼                     ▼                     ▼
   ┌─────────────────────────────────────────────────────────┐
   │                      Application                         │  use-case handlers, behaviors
   └──────────────┬───────────────────────────┬──────────────┘
                  ▼                           ▼
          ┌──────────────┐           ┌──────────────────┐
          │   Contract   │           │      Domain      │
          │ (DTO, .resx) │──────────▶│ (entity, policy) │
          └──────────────┘           └──────────────────┘
```

| Tầng | Phụ thuộc | Nhiệm vụ |
|---|---|---|
| **Domain** | (không) | Trái tim nghiệp vụ: entity, enum, policy thuần, domain exception. Không biết DB, HTTP, framework. |
| **Contract** | Domain | DTO vào/ra (Command/Query/Response), validator, hằng số, resource `.resx` (song ngữ). |
| **Application** | Domain, Contract | Use-case handler (command/query), pipeline behaviors, interface cho hạ tầng (abstraction). |
| **Persistence** | Application, Domain | EF Core: DbContext, cấu hình entity, migration, interceptor, repository/store. |
| **Infrastructure** | Application, Contract, Domain | Tích hợp ngoài: SePay, VietMap, Cloudinary, mail, Google OAuth, Redis, Quartz. |
| **Presentation** | Application, Contract | Endpoint (Minimal API), nhận request → gọi MediatR → trả response. |
| **API** | tất cả | Host: `Program.cs`, lắp ráp DI, middleware, security, cấu hình. |

**Luật vàng:** Domain không `using` bất kỳ tầng nào khác. Application chỉ nói chuyện với hạ tầng qua **interface** (định nghĩa ở Application, cài đặt ở Persistence/Infrastructure).

---

## 2. Cây thư mục tổng

```
badminton/
├── be_booking_badminton.sln
├── .husky/                         # git hooks (pre-commit, commit-msg, pre-push)
├── docs/
│   ├── database/                   # thiết kế DB 9 cụm + database.md (DBML)
│   └── architecture/               # tài liệu này
└── src/   (hoặc để phẳng như hiện tại)
    ├── be_booking_badminton.Domain/
    ├── be_booking_badminton.Contract/
    ├── be_booking_badminton.Application/
    ├── be_booking_badminton.Persistence/
    ├── be_booking_badminton.Infrastructure/
    ├── be_booking_badminton.Presentation/
    └── be_booking_badminton.API/
```

> Hiện tại solution đang có 4 project (API/Application/Domain/Infrastructure). Bản dựng mới tách thành **7 project** như trên: thêm `Contract`, `Persistence`, `Presentation` để đúng style buildx.

---

## 3. Chi tiết từng project

### 3.1. `Domain` — nghiệp vụ thuần

```
Domain/
├── entities/                 # 37 entity ↔ 37 bảng DB
│   ├── User.cs  UserProfile.cs  Role.cs  Permission.cs  RolePermission.cs
│   ├── UserRole.cs  RefreshToken.cs  UserExternalLogin.cs
│   ├── OwnerRequest.cs  OwnerProfile.cs  UserBankAccount.cs
│   ├── Facility.cs  FacilityImage.cs  FacilityStaff.cs
│   ├── ScheduleTemplate.cs  TemplateSlot.cs  Court.cs
│   ├── CourtSlotBlock.cs  CourtSlotGroup.cs  CourtSlotGroupItem.cs  CourtPriceOverride.cs
│   ├── Booking.cs  BookingDetail.cs
│   ├── Campaign.cs  CampaignCode.cs  CampaignTarget.cs
│   ├── OwnerPaymentAccount.cs  BankTransaction.cs  Commission.cs  CommissionSettlement.cs
│   ├── Feedback.cs  FeedbackReply.cs
│   ├── Report.cs  ReportMessage.cs
│   ├── FavoriteFacility.cs
│   └── Config.cs  Notification.cs
├── enums/                    # UserStatus, BookingStatus, CommissionStatus, ... (25 enum)
├── policies/                 # LUẬT nghiệp vụ thuần, không I/O
│   ├── AvailabilityPolicy.cs       # tính slot trống (5 lớp: template→override→group→block→booking)
│   ├── RefundPolicy.cs             # tính tiền hoàn theo refund_before_hours + người hủy
│   ├── CommissionPolicy.cs         # tính hoa hồng trên các detail COMPLETED
│   └── DiscountPolicy.cs           # áp voucher + phân bổ giảm xuống booking_details
├── abstractions/             # BaseEntity, IAuditableEntity, ISoftDeletable, interface chung
└── exceptions/               # DomainException, MessageCode (mã lỗi dùng cho .resx)
```

- **entity** = vai*không** chứa attribute EF (cấu hình để ở Persistence).
- **policy** = hàm thuần nhận dữ liệu → trả kết quả (dễ unit-test, không chạm DB). Đây là nơi giữ các luật phức tạp đã bàn (chống trùng, hoàn tiền, hoa hồng, voucher).

### 3.2. `Contract` — hợp đồng dữ liệu vào/ra

```
Contract/
├── services/                 # chia theo feature
│   ├── booking/
│   │   ├── Command.cs        # CreateBookingCommand, CancelBookingCommand... (record, implement IRequest)
│   │   ├── Query.cs          # GetBookingQuery, ListMyBookingsQuery...
│   │   ├── Response.cs       # BookingResponse, BookingDetailResponse
│   │   └── validators/       # FluentValidation cho từng Command/Query
│   ├── facility/  schedule/  campaign/  payment/  feedback/  report/  identity/ ...
├── abstractions/             # Result<T>, PagedResult<T>, interface IRequest chung
├── constants/                # hằng số (config key, role key, giới hạn)
├── resources/                # SONG NGỮ: SharedMessages.resx + .vi.resx, validation-messages(.vi).resx
└── templates/                # mẫu email, nội dung notification
```

- Command/Query/Response là **record** đơn giản, không logic. Đây là "ngôn ngữ" giữa Presentation và Application.
- `resources/*.resx` = bảng tra MessageCode → chuỗi theo ngôn ngữ (cơ chế i18n, không nằm ở DB).

### 3.3. `Application` — use-case (CQRS)

```
Application/
├── usecases/
│   ├── commands/             # ghi (tạo/sửa/xóa) — mỗi feature 1 folder
│   │   ├── booking/
│   │   │   ├── CreateBookingCommandHandler.cs
│   │   │   ├── CancelBookingCommandHandler.cs
│   │   │   ├── BookingMapping.cs          # map entity ↔ response
│   │   │   └── BookingAccess.cs           # kiểm tra quyền trên tài nguyên
│   │   ├── payment/  campaign/  facility/ ...
│   │   └── ...
│   └── queries/              # đọc — handler trả Response, không đổi state
│       ├── booking/  facility/ ...
├── behaviors/                # MediatR pipeline, chạy quanh MỌI handler, đúng thứ tự:
│   ├── LoggingBehavior.cs
│   ├── ValidationBehavior.cs          # chạy FluentValidation
│   ├── PerformanceBehavior.cs
│   ├── CachingBehavior.cs             # cache query (Redis)
│   ├── ConstraintViolationBehavior.cs # bắt lỗi unique/exclusion → MessageCode
│   ├── TransactionBehavior.cs         # bọc command trong 1 DB transaction
│   └── TracingBehavior.cs
├── abstractions/             # INTERFACE cho hạ tầng (Application định nghĩa, tầng ngoài cài):
│   ├── IAppDbContext.cs  IUnitOfWork.cs  IPostCommitQueue.cs
│   ├── IPaymentGateway.cs (SePay)  IMapService.cs (VietMap)  IFileStorage.cs (Cloudinary)
│   ├── IEmailSender.cs  ICacheService.cs  ICurrentUser.cs  ITokenService.cs
│   └── ...
├── services/                 # orchestration dùng chung nhiều handler
└── dependencyInjection/      # AddApplication() — đăng ký MediatR, behaviors, validators
```

- **1 handler = 1 use-case.** Command đổi state (bọc transaction), Query chỉ đọc.
- Thứ tự behaviors rất quan trọng: Logging → Validation → Performance → Caching → ConstraintViolation → Transaction → Tracing.

### 3.4. `Persistence` — EF Core + PostgreSQL

```
Persistence/
├── AppDbContext.cs           # DbSet cho 37 entity, cài IAppDbContext
├── configurations/           # mỗi entity 1 file IEntityTypeConfiguration
│   ├── UserConfiguration.cs  BookingConfiguration.cs  BookingDetailConfiguration.cs
│   ├── CommissionConfiguration.cs ...   # map cột, index, CHECK, exclusion constraint, FK
├── Migrations/               # EF migration (dotnet ef migrations add ...)
├── interceptors/             # tự động: AuditInterceptor (created/updated_at), SoftDeleteInterceptor
├── repositories/             # store cho thao tác đặc biệt (row lock FOR UPDATE, đếm lượt voucher)
│   ├── BookingStore.cs  CampaignCodeLocker.cs  BankTransactionStore.cs ...
├── constants/                # tên bảng, tên constraint
└── dependencyInjection/      # AddPersistence() — DbContext, Npgsql, interceptor, repository
```

- Đa số CRUD đi thẳng qua `IAppDbContext` trong handler; **repository chỉ tạo khi cần SQL đặc biệt** (khóa dòng, exclusion, đếm nguyên tử) — không tạo repository cho mọi entity.
- Toàn bộ CHECK/unique/exclusion constraint (đã thiết kế ở `docs/database`) khai báo trong `configurations/`.

### 3.5. `Infrastructure` — tích hợp ngoài

```
Infrastructure/
├── payments/                 # SePay: verify webhook (HMAC), tạo QR, khớp giao dịch
│   ├── SePayGateway.cs  SePayWebhookVerifier.cs
├── maps/                     # VietMap: geocode, tính khoảng cách
├── files/                    # Cloudinary: upload ảnh cơ sở/sân
├── authentication/           # JWT phát/validate, Google OAuth, SePay OAuth (owner connect)
│   ├── TokenService.cs  GoogleAuthService.cs  SePayOAuthService.cs
├── passwordHasher/           # băm mật khẩu
├── mail/                     # gửi email (xác thực, thông báo)
├── caching/                  # Redis: ICacheService, cache availability
├── backgroundJobs/           # Quartz: job hết hạn booking, completed, gom đối soát, overdue
│   ├── ExpirePendingBookingJob.cs  CompleteBookingJob.cs
│   ├── GenerateSettlementJob.cs    MarkOverdueSettlementJob.cs
└── dependencyInjection/      # AddInfrastructure() — đăng ký mọi service ngoài
```

- Mỗi service ngoài **cài đặt 1 interface** đã khai ở `Application/abstractions`. Đổi nhà cung cấp (vd SePay → cổng khác) chỉ sửa ở đây.

### 3.6. `Presentation` — endpoint

```
Presentation/
├── apis/                     # mỗi feature 1 file, nhóm endpoint
│   ├── booking/
│   │   ├── BookingApi.cs         # POST /bookings, GET /bookings/{id}, POST /bookings/{id}/cancel
│   │   └── BookingRequest.cs     # model nhận từ HTTP body (map sang Command)
│   ├── facility/  payment/ (SePayWebhookApi.cs)  campaign/  feedback/ ...
└── abstractions/             # base cho module endpoint, helper map Result<T> → HTTP status
```

- Endpoint **mỏng**: nhận request → tạo Command/Query → `mediator.Send` → map `Result<T>` ra HTTP. Không có logic nghiệp vụ.
- Webhook SePay nằm ở đây (`SePayWebhookApi.cs`): chỉ verify + ghi `bank_transactions`, nghiệp vụ để worker xử.

### 3.7. `API` — host

```
API/
├── Program.cs                # điểm vào: build + run, gọi Add* của mọi tầng
├── dependencyInjection/
│   ├── extensions/           # ServiceCollectionExtensions (JWT, CORS, rate limit, localization)
│   ├── options/              # Options pattern + ValidateOnStart
│   └── openapi/              # cấu hình Swagger
├── middlewares/              # ExceptionHandlingMiddleware (map DomainException → MessageCode → .resx)
├── security/                 # CORS allowlist, CSRF origin, rate-limit policy
├── startup/                  # guard chạy lúc khởi động (seed permission, bootstrap admin)
├── Properties/               # launchSettings
├── appsettings.json          # cấu hình (connection string, SePay, Redis, JWT...)
└── appsettings.Development.json
```

---

## 4. Một lát cắt feature đi xuyên các tầng (ví dụ: tạo Booking)

```
POST /bookings
  │
  ▼ Presentation/apis/booking/BookingApi.cs
      nhận BookingRequest → dựng CreateBookingCommand → mediator.Send
  │
  ▼ Application/behaviors  (Validation → ... → Transaction bọc toàn bộ)
  │
  ▼ Application/usecases/commands/booking/CreateBookingCommandHandler.cs
      - đọc config (advance_booking_days, min_booking_minutes)
      - gọi Domain/policies/AvailabilityPolicy + DiscountPolicy
      - khóa dòng campaign_codes (Persistence/repositories/CampaignCodeLocker)
      - tạo Booking + BookingDetail qua IAppDbContext
      - exclusion constraint (Persistence/configurations) chặn trùng
      - tạo QR qua IPaymentGateway (Infrastructure/payments/SePayGateway)
  │
  ▼ Contract/services/booking/Response.cs  →  Result<BookingResponse>
  │
  ▼ Presentation map Result → 201 Created / lỗi → MessageCode → .resx (song ngữ)
```

**Cùng tên feature `booking` xuất hiện ở 4 tầng:** Contract (DTO) · Application (handler) · Persistence (config/store) · Presentation (api). Đọc dọc theo tên feature là thấy trọn nghiệp vụ.

---

## 5. Quy ước đặt tên

| Loại | Mẫu | Ví dụ |
|---|---|---|
| Command | `<Việc>Command` | `CreateBookingCommand` |
| Command handler | `<Việc>CommandHandler` | `CreateBookingCommandHandler` |
| Query | `<Việc>Query` | `ListMyBookingsQuery` |
| Response | `<Thứ>Response` | `BookingResponse` |
| Entity config | `<Entity>Configuration` | `BookingConfiguration` |
| API module | `<Feature>Api` | `BookingApi` |
| Policy thuần | `<Chủ đề>Policy` | `RefundPolicy` |
| Interface hạ tầng | `I<Dịch vụ>` | `IPaymentGateway` |
| Folder feature | camelCase | `booking`, `ownerPaymentAccount` |

---

## 6. Ánh xạ 9 cụm DB → feature folder

| Cụm DB | Feature folder (Application/Contract/Presentation) |
|---|---|
| User / phân quyền | `identity`, `role`, `auth` |
| Owner | `owner` (request + profile + payment account) |
| Setup sân | `facility`, `schedule` (template/slot/court/block/group/override), `facilityStaff` |
| Booking | `booking` |
| Campaign | `campaign` |
| Giao dịch / hoa hồng | `payment` (webhook), `commission`, `settlement` |
| Feedback | `feedback` |
| Report | `report` |
| Yêu thích | `favorite` |
| Hệ thống | `config`, `notification` |

---

> **Bước tiếp theo:** tạo 7 project theo cây này, cài DbContext + 37 entity + configuration, chạy migration đầu tiên khớp `docs/database/database.md`, rồi làm từng feature slice bắt đầu từ `identity` → `facility`/`schedule` → `booking` → `payment`.
