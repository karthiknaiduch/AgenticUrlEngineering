namespace AgenticEngineering.Domain.Enums;

public enum FailureType
{
    Unknown,
    Transient,
    Validation,
    PolicyViolation,
    DependencyFailure,
    SecurityFailure,
    ImplementationFailure
}