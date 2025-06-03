using Microsoft.AspNetCore.Authorization.Infrastructure;

public static class ResourceOperations
{
    public static OperationAuthorizationRequirement Create = new() { Name = "Create" };
    public static OperationAuthorizationRequirement Read = new() { Name = "Read" };
    public static OperationAuthorizationRequirement Update = new() { Name = "Update" };
    public static OperationAuthorizationRequirement Delete = new() { Name = "Delete" };
}