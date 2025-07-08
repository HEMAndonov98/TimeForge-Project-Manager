namespace TimeForge.Common.Dto_Validation;

public static class TagDtoValidationConstants
{
    public const int TagNameMaxLength = 20;

    public const int TagNameMinLength = 3;
    
    public const string TagNameRequiredErrorMessage = "Tag name is required";

    public const string TagNameLengthErrorMessage = "The tag name must be between {1} and {0} characters long";
}