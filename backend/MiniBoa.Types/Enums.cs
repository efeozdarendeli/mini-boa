namespace MiniBoa.Types;

// Enum isimleri veritabanındaki CHECK kısıtlarındaki metinlerle birebir aynıdır;
// Dapper okurken metni enum'a çevirir, yazarken repository'ler .ToString() gönderir.

public enum CustomerStatus
{
    Active,
    Passive
}

public enum ApplicationStatus
{
    Started,
    PersonCreated,
    Completed,
    Cancelled
}

public enum AccountType
{
    Current,
    Participation
}

public enum AccountStatus
{
    Active,
    Closed
}

public enum TransactionDirection
{
    Debit,
    Credit
}
