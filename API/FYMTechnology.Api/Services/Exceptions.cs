namespace FYMTechnology.Api.Services;

public class ConflictException(string message) : Exception(message);

public class ValidationException(string message) : Exception(message);
