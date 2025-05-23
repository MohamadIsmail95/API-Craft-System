namespace ApiCraftSystem.Helper.Enums
{
    public enum SqlServerDataType
    {
        // Numeric types
        BigInt,
        Int,
        SmallInt,
        TinyInt,
        Decimal,
        Numeric,
        Float,
        Real,
        Money,
        SmallMoney,

        // Character types
        Char,
        VarChar,
        NChar,
        NVarChar,
        Text,          // Deprecated
        NText,         // Deprecated

        // Date and time types
        Date,
        DateTime,
        DateTime2,
        SmallDateTime,
        Time,
        DateTimeOffset,

        // Binary types
        Binary,
        VarBinary,
        Image,         // Deprecated
        RowVersion,    // synonym for Timestamp in SQL Server

        // Unique identifiers
        UniqueIdentifier,

        // Other
        SqlVariant,
        Xml,
        Json           // Stored as NVARCHAR
    }

}
