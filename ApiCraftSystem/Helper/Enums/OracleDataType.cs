namespace ApiCraftSystem.Helper.Enums
{
    public enum OracleDataType
    {
        // Numeric types
        Number,
        BinaryFloat,
        BinaryDouble,

        // Character types
        Char,
        NChar,
        VarChar2,
        NVarChar2,
        Clob,
        NClob,
        Long,

        // Date and time types
        Date,
        Timestamp,
        TimestampWithTimeZone,
        TimestampWithLocalTimeZone,
        IntervalYearToMonth,
        IntervalDayToSecond,

        // Binary types
        Raw,
        LongRaw,
        Blob,
        BFile,

        // Unique identifiers
        RowId,
        URowId,

        // Other
        XmlType,
        Json,          // Oracle 21c and later
        Udt,
        Boolean// User-defined type
    }

}
