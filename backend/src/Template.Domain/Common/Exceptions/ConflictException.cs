namespace Template.Domain.Common.Exceptions;

// Convertida em 409 pelo GlobalExceptionHandler (ADR 0006)
public class ConflictException(string message) : Exception(message);
