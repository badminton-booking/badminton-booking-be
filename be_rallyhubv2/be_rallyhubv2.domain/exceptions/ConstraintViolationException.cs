namespace be_rallyhubv2.domain.exceptions;

public sealed class ConstraintViolationException(ConstraintViolationKind kind,
    string constraintName, Exception? inner = null)
: Exception($"{kind} constraint '{constraintName}' was violated.", inner)
{
    public ConstraintViolationKind Kind { get; } = kind;
    public string ConstraintName { get; } =  constraintName;
}

//EfUnitOfWork(persistence) biết Postgres và mã ỗi, nhưng không biết nghiệu vụ nên không biết "ux_users_email" nghĩa gì
//ConstraintViolationBehavior(appplication) biết nghiệp vụ, nhưng khoogn biết  được PostgreSQL
//ConstraintViolationException là cầu nối chỉ mang  loại lỗi + tên constraint