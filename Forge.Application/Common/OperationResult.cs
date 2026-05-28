namespace Forge.Application.Common
{
    public enum OperationStatus
    {
        Ok,
        BadRequest,
        Unauthorized,
        NotFound,
        Conflict
    }

    public sealed record ValidationError(string Field, string Message);

    public sealed record OperationResult<T>(
        OperationStatus Status,
        T? Value,
        string? Message,
        IReadOnlyList<ValidationError>? Errors = null)
    {
        public static OperationResult<T> Ok(T value, string? message = null)
            => new(OperationStatus.Ok, value, message);

        public static OperationResult<T> BadRequest(string message)
            => new(OperationStatus.BadRequest, default, message);

        public static OperationResult<T> ValidationFailed(IReadOnlyList<ValidationError> errors)
            => new(OperationStatus.BadRequest, default, "Walidacja nie powiodła się.", errors);

        public static OperationResult<T> Unauthorized(string message)
            => new(OperationStatus.Unauthorized, default, message);

        public static OperationResult<T> NotFound(string? message = null)
            => new(OperationStatus.NotFound, default, message);

        public static OperationResult<T> Conflict(string message)
            => new(OperationStatus.Conflict, default, message);
    }
}
