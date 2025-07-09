namespace ProjectTemplate.Domain.Exceptions;
public class ModelIsNullException(string modelName, string comment = "") : Exception($"{modelName} is null!" + comment);

