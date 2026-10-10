namespace Template.Domain.Common.Exceptions;

// Convertida em 404 pelo GlobalExceptionHandler (ADR 0006)
public class NotFoundException(string message) : Exception(message);
