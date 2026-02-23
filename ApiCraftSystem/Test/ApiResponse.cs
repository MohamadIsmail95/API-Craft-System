namespace ApiCraftSystem.Test
{
    public class ApiResponse<T>
    {
        public T? Data { get; set; }
        public bool Succeeded { get; set; }
        public object? Errors { get; set; }
        public string Message { get; set; } = string.Empty;
        public int Code { get; set; }
        public int Count { get; set; }

        public static ApiResponse<T> Success(T data, int count = 0, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Data = data,
                Succeeded = true,
                Message = message,
                Code = 0,
                Count = count
            };
        }

        public static ApiResponse<T> Fail(object errors, string message = "Failed", int code = -1)
        {
            return new ApiResponse<T>
            {
                Succeeded = false,
                Errors = errors,
                Message = message,
                Code = code
            };
        }
    }
}
